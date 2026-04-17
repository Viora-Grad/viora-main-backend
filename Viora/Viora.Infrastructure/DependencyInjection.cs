using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Viora.Application.Abstractions.Clock;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;
using Viora.Domain.Subscriptions;
using Viora.Infrastructure.Clock;
using Viora.Infrastructure.Repositories;

namespace Viora.Infrastructure;

internal class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(IServiceCollection services, IConfiguration configuration)
    {

        var ConnectionString = configuration.GetConnectionString("Database");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(ConnectionString));
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IFeatureUsageRepository, FeatureUsageRepository>();
        services.AddScoped<IPlanFeatureRepository, PlanFeatureRepository>();
        services.AddScoped<IOrganizationRepository, OrganziationRepository>();
        services.AddScoped<ILimitedFeatureRepository, LimitedFeatureRepository>();
        services.AddScoped<IFeatureRepository, FeatureRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

        services.AddTransient<IDateTimeProvider, DateTimeProvider>();

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
        return services;

    }
}
