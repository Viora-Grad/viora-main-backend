using DotNetEnv;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Viora.Infrastructure;

// used as a reference in design time during migration to run the env and check the model, is over written in update time with docker env's
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var env = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
        if (File.Exists(env))
        {
            Env.Load(env);
        }
        var password = Environment.GetEnvironmentVariable("SA_PASSWORD") ?? "YoWassup123!PASS";
        var connectionString = $"Server=localhost,1433;Database=Viora;User Id=sa;Password={password};TrustServerCertificate=True;";
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString,
                x => x.UseNetTopologySuite())
            .Options;
        return new ApplicationDbContext(options, new NullPublisher());
    }

    private class NullPublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;
    }
}