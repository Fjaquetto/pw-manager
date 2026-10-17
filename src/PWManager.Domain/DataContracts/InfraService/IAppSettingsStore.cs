using PWManager.Domain.Model;

namespace PWManager.Domain.DataContracts.InfraService;

public interface IAppSettingsStore
{
    AppSettings Load();

    void Save(AppSettings settings);
}
