using PWManager.Domain.Model;

namespace PWManager.Services.Interfaces;

/// <summary>
/// The user preferences, cached and persisted outside the vault database.
/// </summary>
public interface IAppSettingsService
{
    AppSettings Current { get; }

    void Update(AppSettings settings);
}
