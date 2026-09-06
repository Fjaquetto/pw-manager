using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PWManager.ViewModels;

public partial class EntryEditorViewModel : ViewModelBase
{
    private string _originalSite = string.Empty;
    private string _originalLogin = string.Empty;
    private string _originalPassword = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDirty))]
    private string _site = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDirty))]
    private string _login = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDirty))]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PasswordMask), nameof(VisibilityLabel))]
    private bool _isPasswordVisible;

    [ObservableProperty]
    private string _siteError = string.Empty;

    [ObservableProperty]
    private string _loginError = string.Empty;

    [ObservableProperty]
    private string _passwordError = string.Empty;

    public Guid? Id
    { get; private set; }

    public bool IsEditing => Id.HasValue;

    public string Title => IsEditing ? "Edit entry" : "New entry";

    public string SaveLabel => IsEditing ? "Save changes" : "Save entry";

    public bool IsDirty => Site != _originalSite || Login != _originalLogin || Password != _originalPassword;

    public char PasswordMask => IsPasswordVisible ? '\0' : '•';

    public string VisibilityLabel => IsPasswordVisible ? "Hide password" : "Show password";

    partial void OnSiteChanged(string value) => SiteError = string.Empty;

    partial void OnLoginChanged(string value) => LoginError = string.Empty;

    partial void OnPasswordChanged(string value) => PasswordError = string.Empty;

    [RelayCommand]
    private void TogglePasswordVisibility() => IsPasswordVisible = !IsPasswordVisible;

    public void Open(PasswordEntryViewModel? entry = null)
    {
        Id = entry?.Id;
        _originalSite = entry?.Site ?? string.Empty;
        _originalLogin = entry?.Login ?? string.Empty;
        _originalPassword = entry?.Password ?? string.Empty;
        Site = _originalSite;
        Login = _originalLogin;
        Password = _originalPassword;
        IsPasswordVisible = false;
        ClearValidationErrors();
        NotifyEditorStateChanged();
    }

    public bool Validate()
    {
        SiteError = string.IsNullOrWhiteSpace(Site) ? "Enter a site or a name for this entry." : string.Empty;
        LoginError = string.IsNullOrWhiteSpace(Login) ? "Enter a login." : string.Empty;
        PasswordError = string.IsNullOrWhiteSpace(Password) ? "Enter or generate a password." : string.Empty;

        return string.IsNullOrEmpty(SiteError) && string.IsNullOrEmpty(LoginError) && string.IsNullOrEmpty(PasswordError);
    }

    public void MakeNew()
    {
        Id = null;
        NotifyEditorStateChanged();
    }

    private void ClearValidationErrors()
    {
        SiteError = string.Empty;
        LoginError = string.Empty;
        PasswordError = string.Empty;
    }

    private void NotifyEditorStateChanged()
    {
        OnPropertyChanged(nameof(IsDirty));
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(SaveLabel));
    }
}
