using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Viora.Application.Abstractions.Behaviors;
using Viora.Application.Subscriptions.AddAddon;
using Viora.Application.Subscriptions.ChangeSubscription;
using Viora.Application.Subscriptions.CreateSubscriptions;
using Viora.Application.Subscriptions.RenewSubscriptions;
using Viora.Domain.Plans.Services;
﻿using Microsoft.Extensions.DependencyInjection;
using Viora.Application.Abstractions.Behaviors;

namespace Viora.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
            typeof(SubscriptionCreatedDomainEventHandler).Assembly,
            typeof(SubscriptionRenewedDomainEventHandler).Assembly,
            typeof(SubscriptionPlanChangeDomainEventHandler).Assembly,
            typeof(AddonAddedDomainEventHandler).Assembly
            ));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LimitedFeaturePipelineBehavior<,>));
        services.AddScoped<ILimitedFeatureUsageService, LimitedFeatureUsageService>();
        // register application services here

        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(QueryCachingBehavior<,>));
        });
        return services;
    }
}
