using System;
using Avalonia.Controls;

namespace PWManager.Views;

public partial class UnlockView : Window
{
    public UnlockView()
    {
        InitializeComponent();
        Opened += (_, _) =>
        {
            var screen = Screens.ScreenFromWindow(this);

            if (screen is not null)
            {
                Width = Math.Min(Width, screen.WorkingArea.Width / screen.Scaling);
                Height = Math.Min(Height, screen.WorkingArea.Height / screen.Scaling - 40);
            }

            this.FindControl<TextBox>("PasswordBox")?.Focus();
        };
    }
}
