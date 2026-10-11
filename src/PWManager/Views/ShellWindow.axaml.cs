using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using PWManager.ViewModels;

namespace PWManager.Views;

/// <summary>
/// The single application window. Keeps the unlock screen while locked, then
/// grows to the vault size with a short eased transition that keeps the window
/// centered on the screen, and reveals the vault with a short fade. Window-level
/// concerns (screen clamping, focus, keyboard shortcuts and close confirmation)
/// live here; the panels only render their state.
/// </summary>
public partial class ShellWindow : Window
{
    private const double VaultWidth = 1180;
    private const double VaultHeight = 780;
    private const double VerticalScreenMargin = 40;

    private static readonly TimeSpan ResizeDuration = TimeSpan.FromMilliseconds(240);
    private static readonly TimeSpan ResizeFrameInterval = TimeSpan.FromMilliseconds(16);

    private ShellViewModel? _viewModel;
    private MainViewModel? _vault;
    private bool _closeApproved;
    private bool _isClosing;
    private bool _isResizing;

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
            _ = ResizeForVaultAsync();
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
            Height = Math.Min(Height, screen.WorkingArea.Height / screen.Scaling - VerticalScreenMargin);
        }
    }

    /// <summary>
    /// Grows the window to the vault size with an eased transition. The window
    /// keeps its center fixed on the screen while it grows, so the vault always
    /// ends up centered instead of anchored to its previous top-left corner.
    /// </summary>
    private async Task ResizeForVaultAsync()
    {
        if (_isResizing || _isClosing)
        {
            return;
        }

        var screen = Screens.ScreenFromWindow(this);

        if (screen is null)
        {
            // No screen information (some headless hosts): fall back to an instant resize.
            Width = VaultWidth;
            Height = VaultHeight;
            return;
        }

        var plan = ResizePlan.FromWindow(this, screen);

        if (plan.IsNoOp)
        {
            return;
        }

        _isResizing = true;
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            while (!_isClosing)
            {
                var elapsed = Stopwatch.GetElapsedTime(startedAt);
                var progress = elapsed >= ResizeDuration ? 1 : elapsed / ResizeDuration;

                ApplyResize(plan, EaseOutCubic(progress));

                if (progress >= 1)
                {
                    break;
                }

                await Task.Delay(ResizeFrameInterval);
            }
        }
        finally
        {
            _isResizing = false;
        }
    }

    private void ApplyResize(ResizePlan plan, double progress)
    {
        var (width, height, position) = plan.At(progress);

        Width = width;
        Height = height;
        Position = position;
    }

    private static double EaseOutCubic(double progress)
    {
        var inverse = 1 - progress;

        return 1 - inverse * inverse * inverse;
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
        _isClosing = true;

        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= OnShellPropertyChanged;
        }

        var vault = _vault;
        AttachVault(null);

        vault?.Dispose();

        base.OnClosed(e);
    }

    /// <summary>
    /// The geometry of one window transition: the size and the physical screen
    /// center at the start and at the end, plus the screen scaling needed to
    /// convert logical sizes to physical pixels.
    /// </summary>
    private readonly record struct ResizePlan(
        double StartWidth,
        double StartHeight,
        double StartCenterX,
        double StartCenterY,
        double TargetWidth,
        double TargetHeight,
        double TargetCenterX,
        double TargetCenterY,
        double Scaling)
    {
        public bool IsNoOp =>
            Math.Abs(StartWidth - TargetWidth) < 0.5 &&
            Math.Abs(StartHeight - TargetHeight) < 0.5 &&
            Math.Abs(StartCenterX - TargetCenterX) < 0.5 &&
            Math.Abs(StartCenterY - TargetCenterY) < 0.5;

        public static ResizePlan FromWindow(Window window, Screen screen)
        {
            var scaling = screen.Scaling > 0 ? screen.Scaling : 1;
            var targetWidth = Math.Min(VaultWidth, screen.WorkingArea.Width / scaling);
            var targetHeight = Math.Min(VaultHeight, screen.WorkingArea.Height / scaling - VerticalScreenMargin);

            return new ResizePlan(
                window.Width,
                window.Height,
                window.Position.X + window.Width * scaling / 2.0,
                window.Position.Y + window.Height * scaling / 2.0,
                targetWidth,
                targetHeight,
                screen.WorkingArea.X + screen.WorkingArea.Width / 2.0,
                screen.WorkingArea.Y + screen.WorkingArea.Height / 2.0,
                scaling);
        }

        public (double Width, double Height, PixelPoint Position) At(double progress)
        {
            var width = StartWidth + (TargetWidth - StartWidth) * progress;
            var height = StartHeight + (TargetHeight - StartHeight) * progress;
            var centerX = StartCenterX + (TargetCenterX - StartCenterX) * progress;
            var centerY = StartCenterY + (TargetCenterY - StartCenterY) * progress;

            return (width, height, new PixelPoint(
                (int)Math.Round(centerX - width * Scaling / 2.0),
                (int)Math.Round(centerY - height * Scaling / 2.0)));
        }
    }
}
