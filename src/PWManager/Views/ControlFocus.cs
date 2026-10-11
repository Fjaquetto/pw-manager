using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace PWManager.Views;

/// <summary>
/// Focuses a named control inside a view, once layout has settled.
/// </summary>
internal static class ControlFocus
{
    public static void Request(Visual root, string name) => Dispatcher.UIThread.Post(() =>
    {
        var control = root.GetVisualDescendants().OfType<Control>().FirstOrDefault(candidate => candidate.Name == name);

        if (control?.IsEffectivelyVisible == true && control.IsEffectivelyEnabled)
        {
            control.Focus();

            if (control is TextBox textBox && name == "SearchBox")
            {
                textBox.SelectAll();
            }
        }
    }, DispatcherPriority.Loaded);
}
