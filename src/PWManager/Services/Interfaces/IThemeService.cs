using PWManager.Domain.Model;

namespace PWManager.Services.Interfaces;

/// <summary>
/// Applies an appearance scheme to the running application.
/// </summary>
public interface IThemeService
{
    void Apply(ColorScheme scheme);
}
