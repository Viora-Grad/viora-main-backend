using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Viora.Application.Abstractions.Behaviors;
using Viora.Domain.Plans.Services;

namespace Viora.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LimitedFeaturePipelineBehavior<,>));
        services.AddScoped<ILimitedFeatureUsageService, LimitedFeatureUsageService>();
        return services;
    }
}
