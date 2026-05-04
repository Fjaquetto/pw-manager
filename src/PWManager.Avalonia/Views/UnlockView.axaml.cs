using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PWManager.Avalonia.ViewModels;

namespace PWManager.Avalonia.Views;

public partial class UnlockView : Window
{
    public UnlockView()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TogglePassword_Click(object? sender, RoutedEventArgs e)
    {
        var box = this.FindControl<TextBox>("PasswordBox");
        if (box != null)
            box.PasswordChar = box.PasswordChar == '\0' ? '•' : '\0';
    }

    private void Header_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }
}

