using Microsoft.Extensions.DependencyInjection;
using Viora.Application.Abstractions.Behaviors;
using Viora.Domain.Plans.Services;

namespace Viora.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        #region IndependentServices

        services.AddScoped<ILimitedFeatureUsageService, LimitedFeatureUsageService>();

        #endregion IndependentServices

        #region Mediator

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            #region BehaviorPipeline
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LimitedFeaturePipelineBehavior<,>));
            cfg.AddOpenBehavior(typeof(QueryCachingBehavior<,>));
            #endregion BehaviorPipeline
        });

        #endregion Mediator

        return services;
    }
}
