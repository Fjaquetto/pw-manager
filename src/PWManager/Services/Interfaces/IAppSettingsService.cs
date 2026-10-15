using PWManager.Domain.Model;

namespace PWManager.Services.Interfaces;

public interface IAppSettingsService
{
    AppSettings Current { get; }

    void Update(AppSettings settings);
}
