using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Viora.Infrastructure;

namespace Viora.Test.Compenents.Infrastructure;

/// <summary>
/// Provides a shared InMemory ApplicationDbContext for infrastructure tests.
/// Note: EF Core InMemory provider has limitations with ComplexProperty (Money) materialization.
/// Tests that query back entities with Money should verify via ChangeTracker or count checks.
/// </summary>
public abstract class InfrastructureTestBase : IDisposable
{
    protected ApplicationDbContext DbContext { get; }
    protected Mock<IPublisher> PublisherMock { get; } = new();

    protected InfrastructureTestBase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        DbContext = new ApplicationDbContext(options, PublisherMock.Object);
    }

    public void Dispose()
    {
        DbContext.Dispose();
    }
}
