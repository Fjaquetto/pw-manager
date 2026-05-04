using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PWManager.Application.DataContracts;
using PWManager.Avalonia.Services;
using PWManager.Domain.DataContracts.InfraService;
using PWManager.Domain.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PWManager.Avalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IUserApplication _userApplication;
    private readonly IUserEncryptorService _userEncryptorService;
    private readonly ClipboardService _clipboardService;

    private System.Collections.Generic.List<PasswordEntryViewModel> _allEntries = new();
    private CancellationTokenSource? _toastCts;

    [ObservableProperty] private string _newSite = string.Empty;
    [ObservableProperty] private string _newLogin = string.Empty;
    [ObservableProperty] private string _newPassword = string.Empty;
    [ObservableProperty] private bool _newPasswordVisible;
    [ObservableProperty] private Guid? _editingId;
    [ObservableProperty] private string _editingLabel = string.Empty;

    public bool IsEditingMode => EditingId.HasValue;
    public string SaveButtonText => IsEditingMode ? "Save Changes" : "Save / Insert";
    public string ClearButtonText => IsEditingMode ? "Cancel" : "Clear";

    [ObservableProperty] private string _searchGlobal = string.Empty;
    [ObservableProperty] private string _filterSite = string.Empty;
    [ObservableProperty] private string _filterLogin = string.Empty;

    [ObservableProperty] private string _entryStatusMessage = string.Empty;
    [ObservableProperty] private bool _entryStatusIsSuccess;
    [ObservableProperty] private bool _hasEntryStatus;

    [ObservableProperty] private string _copyToastMessage = string.Empty;
    [ObservableProperty] private bool _showCopyToast;

    public bool EntryStatusIsError => HasEntryStatus && !EntryStatusIsSuccess;

    public ObservableCollection<PasswordEntryViewModel> Entries { get; } = new();

    public PasswordGeneratorViewModel Generator { get; }

    public int EntryCount => _allEntries.Count;

    public MainViewModel(IUserApplication userApplication, IUserEncryptorService userEncryptorService,
        ClipboardService clipboardService)
    {
        _userApplication = userApplication;
        _userEncryptorService = userEncryptorService;
        _clipboardService = clipboardService;
        Generator = new PasswordGeneratorViewModel(clipboardService);
    }

    public async Task LoadEntriesAsync()
    {
        var users = await _userApplication.GetAllUsersAsync();
        _allEntries = users
            .Select(u =>
            {
                var decrypted = _userEncryptorService.DecryptUser(CloneUser(u));
                return PasswordEntryViewModel.FromModel(decrypted);
            })
            .ToList();
        OnPropertyChanged(nameof(EntryCount));
        ApplyFilter();
    }

    partial void OnSearchGlobalChanged(string value) => ApplyFilter();
    partial void OnFilterSiteChanged(string value) => ApplyFilter();
    partial void OnFilterLoginChanged(string value) => ApplyFilter();

    partial void OnEditingIdChanged(Guid? value)
    {
        OnPropertyChanged(nameof(IsEditingMode));
        OnPropertyChanged(nameof(SaveButtonText));
        OnPropertyChanged(nameof(ClearButtonText));
        if (value is null) EditingLabel = string.Empty;
    }

    private void ApplyFilter()
    {
        var q = _allEntries.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(SearchGlobal))
        {
            var term = SearchGlobal.ToUpperInvariant();
            q = q.Where(e => e.Site.ToUpperInvariant().Contains(term) || e.Login.ToUpperInvariant().Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(FilterSite))
            q = q.Where(e => e.Site.ToUpperInvariant().Contains(FilterSite.ToUpperInvariant()));
        if (!string.IsNullOrWhiteSpace(FilterLogin))
            q = q.Where(e => e.Login.ToUpperInvariant().Contains(FilterLogin.ToUpperInvariant()));

        Entries.Clear();
        foreach (var e in q)
            Entries.Add(e);
    }

    [RelayCommand]
    private void ClearFilters()
    {
        FilterSite = string.Empty;
        FilterLogin = string.Empty;
        SearchGlobal = string.Empty;
    }

    [RelayCommand]
    private void UseGeneratedPassword()
    {
        NewPassword = Generator.GeneratedPassword;
    }

    private void ShowStatus(string message, bool success, int autoDismissMs = 0)
    {
        EntryStatusMessage = message;
        EntryStatusIsSuccess = success;
        HasEntryStatus = true;
        OnPropertyChanged(nameof(EntryStatusIsError));

        if (autoDismissMs > 0)
        {
            _ = Task.Delay(autoDismissMs).ContinueWith(_ =>
                Dispatcher.UIThread.Post(() =>
                {
                    HasEntryStatus = false;
                    OnPropertyChanged(nameof(EntryStatusIsError));
                }));
        }
    }

    [RelayCommand]
    private void ClearEntry()
    {
        NewSite = string.Empty;
        NewLogin = string.Empty;
        NewPassword = string.Empty;
        NewPasswordVisible = false;
        EditingId = null;
        HasEntryStatus = false;
        EntryStatusMessage = string.Empty;
    }

    [RelayCommand]
    private async Task SaveEntryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewSite) || string.IsNullOrWhiteSpace(NewLogin) || string.IsNullOrWhiteSpace(NewPassword))
        {
            ShowStatus("Please fill in Site, Login and Password.", success: false);
            return;
        }

        if (EditingId.HasValue)
        {
            var existing = await _userApplication.GetUserByIdAsync(EditingId.Value);
            if (existing != null)
            {
                var decrypted = _userEncryptorService.DecryptUser(CloneUser(existing));
                decrypted.Site = NewSite;
                decrypted.Login = NewLogin;
                decrypted.Password = NewPassword;
                decrypted.LastUpdated = DateTime.Now;
                var encrypted = _userEncryptorService.EncryptUser(decrypted);
                await _userApplication.UpdateUserAsync(encrypted);
            }
        }
        else
        {
            var user = new User(NewSite, NewLogin, NewPassword);
            _userEncryptorService.EncryptUser(user);
            await _userApplication.AddUserAsync(user);
        }

        bool isEdit = EditingId.HasValue;
        ClearEntry();
        await LoadEntriesAsync();
        ShowStatus(isEdit ? "Entry updated successfully." : "Entry saved successfully.", success: true, autoDismissMs: 3000);
    }

    [RelayCommand]
    private void EditEntry(PasswordEntryViewModel entry)
    {
        EditingId = entry.Id;
        EditingLabel = entry.Site;
        NewSite = entry.Site;
        NewLogin = entry.Login;
        NewPassword = entry.Password;
    }

    [RelayCommand]
    private async Task DeleteEntryAsync(PasswordEntryViewModel entry)
    {
        var user = await _userApplication.GetUserByIdAsync(entry.Id);
        if (user != null)
        {
            await _userApplication.DeleteUserAsync(user);
            await LoadEntriesAsync();
            ShowStatus("Entry deleted.", success: false, autoDismissMs: 3000);
        }
    }

    [RelayCommand]
    private async Task CopyPasswordAsync(PasswordEntryViewModel entry)
    {
        await _clipboardService.SetTextAsync(entry.Password);
    }

    public async Task CopyTextAsync(string text, string label = "Content")
    {
        await _clipboardService.SetTextAsync(text);
        await ShowCopyToastAsync($"{label} copied to clipboard");
    }

    private async Task ShowCopyToastAsync(string message)
    {
        _toastCts?.Cancel();
        _toastCts = new CancellationTokenSource();
        CopyToastMessage = message;
        ShowCopyToast = true;
        try
        {
            await Task.Delay(2000, _toastCts.Token);
            ShowCopyToast = false;
        }
        catch (OperationCanceledException) { }
    }

    [RelayCommand]
    private void TogglePasswordVisibility(PasswordEntryViewModel entry)
    {
        entry.IsPasswordVisible = !entry.IsPasswordVisible;
    }

    private static User CloneUser(User u) => new()
    {
        Id = u.Id,
        Site = u.Site,
        Login = u.Login,
        Password = u.Password,
        CreationDate = u.CreationDate,
        LastUpdated = u.LastUpdated
    };
}
