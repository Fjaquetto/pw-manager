using PWManager.Domain.Model;

namespace PWManager.Services.Interfaces;

public interface IThemeService
{
    void Apply(ColorScheme scheme);
}
