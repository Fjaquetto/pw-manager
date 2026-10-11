using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PWManager.ViewModels;

/// <summary>
/// Coordinates the single application window: stays on the unlock screen until
/// authentication succeeds, then creates and loads the vault before revealing
/// it. Nothing of the vault exists before a successful unlock.
/// </summary>
public partial class ShellViewModel : ViewModelBase
{
    private readonly Func<MainViewModel> _vaultFactory;
    private MainViewModel? _vault;
    private Task? _openingTask;

    public ShellViewModel(UnlockViewModel unlock, Func<MainViewModel> vaultFactory)
    {
        Unlock = unlock;
        _vaultFactory = vaultFactory;
        Unlock.Unlocked += OnUnlocked;
        Unlock.PropertyChanged += OnUnlockPropertyChanged;
    }

    public UnlockViewModel Unlock { get; }

    /// <summary>
    /// The vault, available only after a successful unlock. Null while locked.
    /// </summary>
    public MainViewModel? Vault
    {
        get => _vault;
        private set => SetProperty(ref _vault, value);
    }

    [ObservableProperty]
    private bool _isOpening;

    [ObservableProperty]
    private bool _isUnlocked;

    /// <summary>
    /// The unlock screen is visible until the vault has been loaded and shown.
    /// </summary>
    public bool ShowUnlock => !IsUnlocked;

    /// <summary>
    /// Covers the whole unlock flow: validating the password and loading the
    /// vault entries, so the user sees a single continuous progress state.
    /// </summary>
    public bool ShowProgress => Unlock.IsBusy || IsOpening;

    /// <summary>
    /// The in-flight vault opening, exposed so tests and the window can await it.
    /// </summary>
    public Task? OpeningTask => _openingTask;

    private void OnUnlocked() => _openingTask = OpenVaultAsync();

    /// <summary>
    /// Creates the vault, loads its entries and only then reveals it.
    /// Idempotent: a second call while opening or after opening does nothing.
    /// </summary>
    public async Task OpenVaultAsync()
    {
        if (_vault is not null || IsUnlocked || IsOpening)
        {
            return;
        }

        IsOpening = true;
        MainViewModel? vault = null;

        try
        {
            vault = _vaultFactory();
            await vault.LoadEntriesAsync();
        }
        finally
        {
            IsOpening = false;
        }

        Vault = vault;
        IsUnlocked = true;
    }

    private void OnUnlockPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(UnlockViewModel.IsBusy))
        {
            OnPropertyChanged(nameof(ShowProgress));
        }
    }

    partial void OnIsOpeningChanged(bool value) => OnPropertyChanged(nameof(ShowProgress));

    partial void OnIsUnlockedChanged(bool value) => OnPropertyChanged(nameof(ShowUnlock));
}
