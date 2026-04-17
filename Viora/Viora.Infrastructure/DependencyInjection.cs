using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Viora.Application.Abstractions.Clock;
using Viora.Infrastructure.Clock;

namespace Viora.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // register services here
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();

        // database context
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (connectionString == null)
            throw new ArgumentNullException(nameof(connectionString));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // register repositories here
        // services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
