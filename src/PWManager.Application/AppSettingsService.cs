using PWManager.Application.DataContracts;
using PWManager.Domain.DataContracts.InfraService;
using PWManager.Domain.Model;

namespace PWManager.Application;

public sealed class AppSettingsService(IAppSettingsStore store) : IAppSettingsService
{
    public AppSettings Current { get; private set; } = store.Load();

    public void Update(AppSettings settings)
    {
        Current = settings.Normalized();
        store.Save(Current);
    }
}
