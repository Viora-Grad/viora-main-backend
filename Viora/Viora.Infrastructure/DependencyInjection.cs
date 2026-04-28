using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Security;
using Viora.Domain.Abstractions;
using Viora.Domain.Users.Customers;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Owners;
using Viora.Infrastructure.Authentication;
using Viora.Infrastructure.Clock;
using Viora.Infrastructure.Repositories.Authentication;
using Viora.Infrastructure.Repositories.Users;
using Viora.Infrastructure.Security;

namespace Viora.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // register services here
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUserContext, UserContext>();

        // database context
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (connectionString == null)
            throw new ArgumentNullException(nameof(connectionString));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // register repositories here
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOwnerRepository, OwnerRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<LocalCredentialRepository>();

        // http context accessor
        services.AddHttpContextAccessor();


        // Unit of Work
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Authentication
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

        return services;
    }
}
