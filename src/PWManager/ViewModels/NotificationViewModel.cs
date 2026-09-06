using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PWManager.ViewModels;

public partial class NotificationViewModel(TimeProvider timeProvider) : ViewModelBase, IDisposable
{
    private CancellationTokenSource? _dismissal;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private bool _isVisible;

    [ObservableProperty]
    private bool _isError;

    public void Show(string message, bool isError = false, bool persistent = false)
    {
        CancelTimer();
        Message = message;
        IsError = isError;
        IsVisible = true;

        if (!isError && !persistent)
        {
            _dismissal = new();
            _ = DismissLaterAsync(_dismissal.Token);
        }
    }

    private async Task DismissLaterAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
            {
                IsVisible = false;
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    [RelayCommand]
    private void Dismiss()
    {
        CancelTimer();
        IsVisible = false;
    }

    private void CancelTimer()
    {
        _dismissal?.Cancel();
        _dismissal?.Dispose();
        _dismissal = null;
    }

    public void Dispose() => CancelTimer();
}
