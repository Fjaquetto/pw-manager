using CommunityToolkit.Mvvm.ComponentModel;
using PWManager.Domain.Model;
using System;

namespace PWManager.Avalonia.ViewModels;

public partial class PasswordEntryViewModel : ViewModelBase
{
    [ObservableProperty] private Guid _id;
    [ObservableProperty] private string _site = string.Empty;
    [ObservableProperty] private string _login = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private DateTime _creationDate;
    [ObservableProperty] private DateTime _lastUpdated;
    [ObservableProperty] private bool _isPasswordVisible;
    [ObservableProperty] private string _maskedPassword = "••••••••••••";
    [ObservableProperty] private bool _isDeletePending;

    public string DisplayPassword => IsPasswordVisible ? Password : "••••••••••••";

    partial void OnIsPasswordVisibleChanged(bool value)
    {
        OnPropertyChanged(nameof(DisplayPassword));
    }

    partial void OnPasswordChanged(string value)
    {
        OnPropertyChanged(nameof(DisplayPassword));
    }

    public static PasswordEntryViewModel FromModel(User user) => new()
    {
        Id = user.Id,
        Site = user.Site,
        Login = user.Login,
        Password = user.Password,
        CreationDate = user.CreationDate,
        LastUpdated = user.LastUpdated
    };
}
