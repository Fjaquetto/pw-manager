using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PWManager.Enums;
using PWManager.Services;
using PWManager.Services.Interfaces;

namespace PWManager.ViewModels;

public partial class UnlockViewModel(IUnlockService unlockService, INavigationService navigation) : ViewModelBase
{
    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PasswordMask), nameof(VisibilityLabel))]
    private bool _isPasswordVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ButtonLabel), nameof(IsIdle))]
    [NotifyCanExecuteChangedFor(nameof(UnlockCommand))]
    private bool _isBusy;

    public char PasswordMask => IsPasswordVisible ? '\0' : '•';

    public string VisibilityLabel => IsPasswordVisible ? "Hide password" : "Show password";

    public bool HasError => ErrorMessage.Length > 0;

    public bool IsIdle => !IsBusy;

    public string ButtonLabel => IsBusy ? "Unlocking…" : "Unlock vault";

    partial void OnPasswordChanged(string value) => ErrorMessage = string.Empty;

    [RelayCommand]
    private void TogglePasswordVisibility() => IsPasswordVisible = !IsPasswordVisible;

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task UnlockAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "Enter your master password.";
            return;
        }

        IsBusy = true;
        var error = string.Empty;

        try
        {
            var result = await unlockService.UnlockAsync(Password);

            if (result == UnlockResult.Success)
            {
                navigation.ShowMain();
            }
            else
            {
                error = result == UnlockResult.InvalidPassword
                    ? "Incorrect master password. Please try again."
                    : "Unable to open your vault. Please try again.";
            }
        }
        catch (Exception)
        {
            error = "Unable to open your vault. Please try again.";
        }
        finally
        {
            Password = string.Empty;
            IsPasswordVisible = false;
            ErrorMessage = error;
            IsBusy = false;
        }
    }
}
