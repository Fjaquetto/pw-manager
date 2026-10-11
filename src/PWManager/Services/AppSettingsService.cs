using PWManager.Domain.Model;
using PWManager.Infra.Config;
using PWManager.Services.Interfaces;

namespace PWManager.Services;

public sealed class AppSettingsService(AppSettingsStore store) : IAppSettingsService
{
    public AppSettings Current { get; private set; } = store.Load();

    public void Update(AppSettings settings)
    {
        Current = settings.Normalized();
        store.Save(Current);
    }
}
