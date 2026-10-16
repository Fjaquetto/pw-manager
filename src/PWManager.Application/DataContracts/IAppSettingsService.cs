using PWManager.Domain.Model;

namespace PWManager.Application.DataContracts;

public interface IAppSettingsService
{
    AppSettings Current { get; }

    void Update(AppSettings settings);
}
