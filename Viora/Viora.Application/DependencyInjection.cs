using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Viora.Application.Abstractions.Behaviors;
using Viora.Application.Reminders.ReminderCreated;
using Viora.Application.Subscriptions.AddAddon;
using Viora.Application.Subscriptions.ChangeSubscription;
using Viora.Application.Subscriptions.CreateSubscriptions;
using Viora.Application.Subscriptions.RenewSubscriptions;
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
            cfg.RegisterServicesFromAssemblies(
                typeof(SubscriptionCreatedDomainEventHandler).Assembly,
                typeof(SubscriptionRenewedDomainEventHandler).Assembly,
                typeof(SubscriptionPlanChangeDomainEventHandler).Assembly,
                typeof(AddonAddedDomainEventHandler).Assembly,
                typeof(ReminderCreatedEventHandler).Assembly
            );
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LimitedFeaturePipelineBehavior<,>));
            cfg.AddOpenBehavior(typeof(QueryCachingBehavior<,>));
            #endregion BehaviorPipeline
        });

        #endregion Mediator

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        return services;
    }
}
