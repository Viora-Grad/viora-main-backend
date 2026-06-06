using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Viora.Infrastructure.Settings;

public static class SettingsRegistration
{
    public static IServiceCollection AddInterfacedOptions<TInterface, TImpl>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName,
        Action<OptionsBuilder<TImpl>>? configure = null)
        where TInterface : class
        where TImpl : class, TInterface, new()
    {
        var optionsBuilder = services.AddOptions<TImpl>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        configure?.Invoke(optionsBuilder);

        services.AddSingleton<TInterface>(sp =>
            sp.GetRequiredService<IOptions<TImpl>>().Value);

        return services;
    }
}