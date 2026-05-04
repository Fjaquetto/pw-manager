using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PWManager.Avalonia.ViewModels;
using System;
using System.Threading.Tasks;

namespace PWManager.Avalonia.Views;

public partial class MainView : Window
{
    private bool _newPasswordVisible;
    private bool _generatorPasswordVisible;

    public MainView()
    {
        InitializeComponent();
    }

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (DataContext is MainViewModel vm)
        {
            await vm.LoadEntriesAsync();
        }
    }

    private void ToggleNewPassword_Click(object? sender, RoutedEventArgs e)
    {
        _newPasswordVisible = !_newPasswordVisible;
        var box = this.FindControl<TextBox>("NewPasswordBox");
        if (box != null)
            box.PasswordChar = _newPasswordVisible ? '\0' : '•';
    }

    private void ToggleGeneratedPassword_Click(object? sender, RoutedEventArgs e)
    {
        _generatorPasswordVisible = !_generatorPasswordVisible;
        var box = this.FindControl<TextBox>("GeneratorPasswordBox");
        if (box != null)
            box.PasswordChar = _generatorPasswordVisible ? '\0' : '•';
    }

    private void ToggleEntryPassword_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is PasswordEntryViewModel entry)
        {
            entry.IsPasswordVisible = !entry.IsPasswordVisible;
        }
    }

    private void DeleteEntry_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is PasswordEntryViewModel entry
            && DataContext is MainViewModel vm)
        {
            foreach (var item in vm.Entries)
                item.IsDeletePending = false;
            entry.IsDeletePending = true;
        }
    }

    private void ConfirmDelete_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is PasswordEntryViewModel entry
            && DataContext is MainViewModel vm)
        {
            vm.DeleteEntryCommand.Execute(entry);
        }
    }

    private async void CopyLogin_Tapped(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBlock tb && tb.DataContext is PasswordEntryViewModel entry
            && DataContext is MainViewModel vm)
            await vm.CopyTextAsync(entry.Login, "Login");
    }

    private async void CopyPassword_Tapped(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBlock tb && tb.DataContext is PasswordEntryViewModel entry
            && DataContext is MainViewModel vm)
            await vm.CopyTextAsync(entry.Password, "Password");
    }

    private void EditEntry_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is PasswordEntryViewModel entry
            && DataContext is MainViewModel vm)
        {
            vm.EditEntryCommand.Execute(entry);
        }
    }

    private void Header_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }
}
