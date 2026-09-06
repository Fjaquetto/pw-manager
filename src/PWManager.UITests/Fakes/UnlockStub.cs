using PWManager.Enums;
using PWManager.Services.Interfaces;

namespace PWManager.UITests.Fakes;

internal sealed class UnlockStub : IUnlockService
{
    public int Calls
    { get; private set; }

    public Task<UnlockResult> UnlockAsync(string password)
    {
        Calls++;

        return Task.FromResult(UnlockResult.InvalidPassword);
    }
}
