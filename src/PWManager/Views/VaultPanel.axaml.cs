using Avalonia.Controls;
using PWManager.ViewModels;

namespace PWManager.Views;

public partial class VaultPanel : UserControl
{
    private MainViewModel? _viewModel;

    public VaultPanel()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (_viewModel is not null)
            {
                _viewModel.FocusRequested -= FocusControl;
            }

            _viewModel = DataContext as MainViewModel;

            if (_viewModel is not null)
            {
                _viewModel.FocusRequested += FocusControl;
            }

            UpdateLayoutMode();
        };
        SizeChanged += (_, _) => UpdateLayoutMode();
    }

    private void FocusControl(string name) => ControlFocus.Request(this, name);

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
}
