using Viora.Infrastructure;
using global::Viora.Infrastructure.Repositories.RealTimeScheduling;

namespace Viora.Test.Compenents.Infrastructure.RealTimeScheduling;

/// <summary>
/// Unit tests for the ScheduleCancellationRepository against an InMemory database.
/// </summary>
[TestClass]
public sealed class ScheduleCancellationRepositoryTests : InfrastructureTestBase
{
    private readonly ScheduleCancellationRepository _repository;

    public ScheduleCancellationRepositoryTests()
    {
        _repository = new ScheduleCancellationRepository(DbContext);
    }

    // ===== Add =====

    [TestMethod]
    public async Task Add_Cancellation_PersistsToDatabase()
    {
        var cancellation = global::Viora.Domain.RealTimeScheduling.ScheduleCancellations.Create(
            Guid.NewGuid(), DateTime.UtcNow, "Staff feeling unwell");

        _repository.Add(cancellation);
        await DbContext.SaveChangesAsync();

        Assert.IsTrue(cancellation.Id != Guid.Empty);
    }

    [TestMethod]
    public async Task Add_MultipleCancellations_PersistsAll()
    {
        var cancel1 = global::Viora.Domain.RealTimeScheduling.ScheduleCancellations.Create(
            Guid.NewGuid(), DateTime.UtcNow, "Reason 1");
        var cancel2 = global::Viora.Domain.RealTimeScheduling.ScheduleCancellations.Create(
            Guid.NewGuid(), DateTime.UtcNow, "Reason 2");

        _repository.Add(cancel1);
        _repository.Add(cancel2);
        await DbContext.SaveChangesAsync();

        Assert.AreNotEqual(cancel1.Id, cancel2.Id);
    }

    // ===== GetByIdAsync (inherited from base) =====

    [TestMethod]
    public async Task GetByIdAsync_CancellationExists_ReturnsCancellation()
    {
        var cancellation = global::Viora.Domain.RealTimeScheduling.ScheduleCancellations.Create(
            Guid.NewGuid(), DateTime.UtcNow, "Emergency leave");
        DbContext.Set<global::Viora.Domain.RealTimeScheduling.ScheduleCancellations>().Add(cancellation);
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(cancellation.Id, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(cancellation.Id, result.Id);
        Assert.AreEqual("Emergency leave", result.Reason);
    }

    [TestMethod]
    public async Task GetByIdAsync_CancellationNotFound_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.IsNull(result);
    }
}
