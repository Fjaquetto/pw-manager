using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PWManager.ViewModels;

namespace PWManager.Views;

public partial class MainView : Window
{
    private MainViewModel? _viewModel;
    private bool _closeApproved;

    public MainView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (_viewModel is not null)
            {
                _viewModel.FocusRequested -= FocusControl;
                _viewModel.CloseRequested -= ApproveClose;
            }

            _viewModel = DataContext as MainViewModel;

            if (_viewModel is not null)
            {
                _viewModel.FocusRequested += FocusControl;
                _viewModel.CloseRequested += ApproveClose;
            }

            UpdateLayoutMode();
        };
        SizeChanged += (_, _) => UpdateLayoutMode();
        AddHandler(KeyDownEvent, HandleKeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
    }

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        var screen = Screens.ScreenFromWindow(this);

        if (screen is not null)
        {
            Width = Math.Min(Width, screen.WorkingArea.Width / screen.Scaling);
            Height = Math.Min(Height, screen.WorkingArea.Height / screen.Scaling - 40);
        }

        if (_viewModel is not null)
        {
            await _viewModel.LoadEntriesAsync();
        }
    }

    private void UpdateLayoutMode()
    {
        if (_viewModel is null)
        {
            return;
        }

        _viewModel.IsCompact = Bounds.Width > 0 && Bounds.Width < 1000;
        var workspace = this.FindControl<Grid>("Workspace")!;
        workspace.ColumnDefinitions = new(_viewModel.IsCompact ? "*" : "360,24,*");
        Grid.SetColumn(this.FindControl<Border>("PaneRegion")!, _viewModel.IsCompact ? 0 : 2);
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F && e.KeyModifiers == KeyModifiers.Control && _viewModel?.CanInteract == true)
        {
            FocusControl("SearchBox");
            e.Handled = true;
        }
    }

    private void FocusControl(string name) => Dispatcher.UIThread.Post(() =>
    {
        var control = this.GetVisualDescendants().OfType<Control>().FirstOrDefault(c => c.Name == name);

        if (control?.IsEffectivelyVisible == true && control.IsEffectivelyEnabled)
        {
            control.Focus();

            if (control is TextBox textBox && name == "SearchBox")
            {
                textBox.SelectAll();
            }
        }
    }, DispatcherPriority.Loaded);

    private void ApproveClose()
    {
        _closeApproved = true;
        Close();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!_closeApproved && _viewModel is not null && !_viewModel.RequestClose())
        {
            e.Cancel = true;
        }

        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.FocusRequested -= FocusControl;
            _viewModel.CloseRequested -= ApproveClose;
            _viewModel.Dispose();
        }

        base.OnClosed(e);
    }
}
