using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Billings;
using Viora.Application.Abstractions.Caching;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Mail;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Scheduling;
using Viora.Application.Abstractions.Security;
using Viora.Domain.Abstractions;
using Viora.Domain.Billings;
using Viora.Domain.Billings.Invoices;
using Viora.Domain.Branches;
using Viora.Domain.ChatSessions;
using Viora.Domain.Feedbacks;
using Viora.Domain.Medias;
using Viora.Domain.Orders;
using Viora.Domain.Organizations.LegalPapers;
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
using Viora.Infrastructure.Authentication;
using Viora.Infrastructure.Caching;
using Viora.Infrastructure.Clock;
using Viora.Infrastructure.Mail;
using Viora.Infrastructure.Media;
using Viora.Infrastructure.Payments;
using Viora.Infrastructure.Repositories;
using Viora.Infrastructure.Repositories.Authentication;
using Viora.Infrastructure.Repositories.Billings;
using Viora.Infrastructure.Repositories.Organizations;
using Viora.Infrastructure.Repositories.Plans;
using Viora.Infrastructure.Repositories.Subscriptions;
using Viora.Infrastructure.Repositories.Users;
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
        services.AddScoped<ILegalPaperRepository, LegalPaperRepository>();
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
        services.AddScoped<ILimitedFeatureRepository, LimitedFeatureRepository>();
        services.AddScoped<IPlanLimitedFeatureRepository, PlanLimitedFeatureRepository>();
        #endregion PlansRepos

        #region UsersRepos
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOwnerRepository, OwnerRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IIdentityRepository, IdentityRepository>();
        services.AddScoped<LocalCredentialRepository>();
        services.AddScoped<RefreshTokenRepository>();
        #endregion UsersRepos

        #region Branches
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        #endregion Branches

        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IChatSessionRepository, ChatSessionRepository>();

        #region BillingRepos
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        #endregion BillingRepos
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
        services.AddScoped<IDevDataSeeder, DevDataSeeder>();
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IGoogleAuthenticator, GoogleAuthenticator>();
        #endregion ServicesRegisters

        #region Payments
        services.AddHttpClient<IPaymentService, KashierPaymentService>((sp, client) =>
        {
            var paymentSettings = sp.GetRequiredService<IPaymentSettings>();
            client.BaseAddress = new Uri(paymentSettings.BaseUrl);
            // Kashier server-side REST: Authorization = Secret (Payment API) key, api-key = GUID API key.
            client.DefaultRequestHeaders.Add("Authorization", paymentSettings.Secret);
            client.DefaultRequestHeaders.Add("api-key", paymentSettings.ApiKey);
            client.DefaultRequestHeaders.Connection.Add("keep-alive");
        });
        #endregion Payments

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
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JWT:ISSUER"],
                ValidAudience = configuration["JWT:AUDIENCE"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["JWT:SECRET"]!)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.AdminOnly, policy => policy.RequireRole("Admin"))
            .AddPolicy(AuthorizationPolicies.OwnerOnly, policy => policy.RequireRole("Owner"))
            .AddPolicy(AuthorizationPolicies.CustomerOnly, policy => policy.RequireRole("Customer"))
            .AddPolicy(AuthorizationPolicies.StaffOnly, policy => policy.RequireRole("Staff").RequireClaim("OrganizationId")) // For tenant-scoping lat
            .AddPermissionPolicies();
        return services;
    }
}
