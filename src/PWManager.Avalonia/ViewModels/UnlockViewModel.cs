using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PWManager.Avalonia.Services;
using PWManager.Domain.DataContracts.InfraService;
using PWManager.Infra.Services;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace PWManager.Avalonia.ViewModels;

public partial class UnlockViewModel : ViewModelBase
{
    private readonly NavigationService _navigationService;

    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private bool _isPasswordVisible;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _hasError;

    public UnlockViewModel(NavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
    }

    [RelayCommand]
    private async Task UnlockAsync()
    {
        if (string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "Please enter your master password.";
            HasError = true;
            return;
        }

        EncryptorService.EncryptorPassword = Password;

        try
        {
            using var scope = App.Services.CreateScope();
            var userApp = scope.ServiceProvider.GetRequiredService<PWManager.Application.DataContracts.IUserApplication>();
            var encryptorService = scope.ServiceProvider.GetRequiredService<IUserEncryptorService>();

            var users = await userApp.GetAllUsersAsync();
            foreach (var user in users)
            {
                encryptorService.DecryptUser(new PWManager.Domain.Model.User
                {
                    Id = user.Id,
                    Site = user.Site,
                    Login = user.Login,
                    Password = user.Password,
                    CreationDate = user.CreationDate,
                    LastUpdated = user.LastUpdated
                });
                break;
            }

            HasError = false;
            ErrorMessage = string.Empty;
            _navigationService.ShowMain();
        }
        catch (CryptographicException)
        {
            EncryptorService.EncryptorPassword = string.Empty;
            ErrorMessage = "Incorrect master password. Please try again.";
            HasError = true;
        }
        catch
        {
            EncryptorService.EncryptorPassword = string.Empty;
            ErrorMessage = "Unable to unlock. Please try again.";
            HasError = true;
        }
        finally
        {
            Password = string.Empty;
        }
    }
}
