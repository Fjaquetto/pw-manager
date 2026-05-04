using Microsoft.Extensions.DependencyInjection;

namespace PWManager.Avalonia;

internal static class ServiceProviderExtensions
{
    public static T GetRequiredService<T>(this System.IServiceProvider provider) where T : notnull
        => (T)provider.GetRequiredService(typeof(T));

    public static IServiceScope CreateScope(this System.IServiceProvider provider)
        => provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
}
