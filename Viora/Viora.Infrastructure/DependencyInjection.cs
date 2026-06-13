using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Caching;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Mail;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Scheduling;
using Viora.Application.Abstractions.Security;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Medias;
using Viora.Domain.Orders;
using Viora.Domain.Organizations.OnBoardings;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Organizations.Suspensions;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;
using Viora.Domain.Shared;
using Viora.Domain.Subscriptions;
using Viora.Domain.Subscriptions.Addons;
using Viora.Domain.Users.Customers;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Owners;
using Viora.Domain.Vivi.ChatSessions;
using Viora.Infrastructure.Authentication;
using Viora.Infrastructure.Caching;
using Viora.Infrastructure.Clock;
using Viora.Infrastructure.Mail;
using Viora.Infrastructure.Firebase;
using Viora.Infrastructure.Media;
using Viora.Infrastructure.Repositories;
using Viora.Infrastructure.Repositories.Authentication;
using Viora.Infrastructure.Repositories.Organizations;
using Viora.Infrastructure.Repositories.Users;
using Viora.Infrastructure.Repositories.Vivi;
using Viora.Infrastructure.Scheduling;
using Viora.Infrastructure.Security;
using Viora.Infrastructure.Seeding;

namespace Viora.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        #region ReposRegisters

        #region OrgRepos
        services.AddScoped<IOrganizationRepository, OrganziationRepository>();
        services.AddScoped<IOrganizationApplicationRepository, OrganizationApplicationRepository>();
        services.AddScoped<ISuspensionRepository, SuspensionRepository>();
        #endregion OrgRepos

        #region PlansRepos
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IFeatureUsageRepository, FeatureUsageRepository>();
        services.AddScoped<IPlanFeatureRepository, PlanFeatureRepository>();
        services.AddScoped<ILimitedFeatureRepository, LimitedFeatureRepository>();
        services.AddScoped<IFeatureRepository, FeatureRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<ILimitedFeatureAddonRepository, LimitedFeatutreAddonRepository>();
        services.AddScoped<ISubscriptionOrderRepository, SubscriptionOrderRepository>();
        services.AddScoped<IAddonOrderRepository, AddonOrderRepository>();
        #endregion PlansRepos

        #region UsersRepos
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOwnerRepository, OwnerRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<LocalCredentialRepository>();
        #endregion UsersRepos

        #region Branches
        services.AddScoped<IBranchRepository, BranchRepository>();
        #endregion Branches

        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
        #endregion ReposRegisters

        #region ServicesRegisters
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        services.AddTransient<ICipher, Cipher>();
        services.AddTransient<IHasher, Hasher>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IHasher, Hasher>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<ITokenValidator, TokenValidator>();
        services.AddScoped<RefreshTokenService>();
        services.AddScoped<IStorageService, StorageService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IDomainEventScheduler, EfDomainEventScheduler>();
        services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
        services.AddScoped<IEmailSender, EmailService>();
        #endregion ServicesRegisters

        #region HostedWorkers
        services.AddHostedService<ScheduledEventDispatcherService>();
        #endregion HostedWorkers

        var connectionString = configuration.GetConnectionString("Default") ?? throw new ArgumentNullException(configuration.GetConnectionString("Default"));

        services.AddDbContext<ApplicationDbContext>(
            options => options.UseSqlServer(
                connectionString,
                x => x.UseNetTopologySuite()));

        services.AddSingleton<IReadOnlyList<Country>>(sp =>
        {
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return db.Set<Country>()
                .AsNoTracking()
                .ToList();
        });

        services.AddSingleton<IReadOnlyList<LimitedFeature>>(sp =>
        {
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return db.Set<LimitedFeature>()
                .AsNoTracking()
                .ToList();
        });

        // register repositories here
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOwnerRepository, OwnerRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<LocalCredentialRepository>();
        services.AddScoped<RefreshTokenRepository>();

        services.AddDistributedMemoryCache();

        services.AddHttpContextAccessor();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.AdminOnly, policy => policy.RequireRole("Admin"))
            .AddPolicy(AuthorizationPolicies.OwnerOnly, policy => policy.RequireRole("Owner"))
            .AddPolicy(AuthorizationPolicies.CustomerOnly, policy => policy.RequireRole("Customer"))
            .AddPolicy(AuthorizationPolicies.StaffOnly, policy => policy.RequireRole("Staff").RequireClaim("OrganizationId")) // For tenant-scoping lat
            .AddPermissionPolicies();

        // Firebase configuration
        services.AddFirebase(configuration);
        services.AddFirebaseMessaging();
        return services;
    }
}
