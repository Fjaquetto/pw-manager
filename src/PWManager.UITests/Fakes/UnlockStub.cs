using PWManager.Enums;
using PWManager.Services.Interfaces;

namespace PWManager.UITests.Fakes;

internal sealed class UnlockStub : IUnlockService
{
    public int Calls { get; private set; }

    public UnlockResult Result { get; set; } = UnlockResult.InvalidPassword;

    public TaskCompletionSource<UnlockResult>? Pending { get; set; }

    public Task<UnlockResult> UnlockAsync(string password)
    {
        Calls++;

        return Pending?.Task ?? Task.FromResult(Result);
    }
}
