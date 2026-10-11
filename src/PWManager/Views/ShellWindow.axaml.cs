using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PWManager.ViewModels;

namespace PWManager.Views;

/// <summary>
/// The single application window. Keeps the unlock screen while locked, then
/// resizes once and reveals the vault with a short fade. Window-level concerns
/// (screen clamping, focus, keyboard shortcuts and close confirmation) live
/// here; the panels only render their state.
/// </summary>
public partial class ShellWindow : Window
{
    private const double VaultWidth = 1180;
    private const double VaultHeight = 780;

    private ShellViewModel? _viewModel;
    private MainViewModel? _vault;
    private bool _closeApproved;

    public ShellWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        AddHandler(KeyDownEvent, HandleKeyDown, RoutingStrategies.Tunnel);
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        ClampToScreen();
        FocusControl(_vault is null ? "PasswordBox" : "SearchBox");
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= OnShellPropertyChanged;
        }

        _viewModel = DataContext as ShellViewModel;

        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged += OnShellPropertyChanged;
            AttachVault(_viewModel.Vault);
        }
    }

    private void OnShellPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(ShellViewModel.Vault))
        {
            AttachVault(_viewModel?.Vault);
        }
        else if (args.PropertyName == nameof(ShellViewModel.IsUnlocked) && _viewModel?.IsUnlocked == true)
        {
            ResizeForVault();
            FocusControl("SearchBox");
        }
    }

    private void AttachVault(MainViewModel? vault)
    {
        if (_vault is not null)
        {
            _vault.CloseRequested -= ApproveClose;
        }

        _vault = vault;

        if (_vault is not null)
        {
            _vault.CloseRequested += ApproveClose;
        }
    }

    private void ClampToScreen()
    {
        var screen = Screens.ScreenFromWindow(this);

        if (screen is not null)
        {
            Width = Math.Min(Width, screen.WorkingArea.Width / screen.Scaling);
            Height = Math.Min(Height, screen.WorkingArea.Height / screen.Scaling - 40);
        }
    }

    private void ResizeForVault()
    {
        Width = VaultWidth;
        Height = VaultHeight;
        ClampToScreen();
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F && e.KeyModifiers == KeyModifiers.Control && _vault?.CanInteract == true)
        {
            FocusControl("SearchBox");
            e.Handled = true;
        }
    }

    private void FocusControl(string name) => ControlFocus.Request(this, name);

    private void ApproveClose()
    {
        _closeApproved = true;
        Close();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!_closeApproved && _vault is not null && !_vault.RequestClose())
        {
            e.Cancel = true;
        }

        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= OnShellPropertyChanged;
        }

        var vault = _vault;
        AttachVault(null);

        vault?.Dispose();

        base.OnClosed(e);
    }
}
