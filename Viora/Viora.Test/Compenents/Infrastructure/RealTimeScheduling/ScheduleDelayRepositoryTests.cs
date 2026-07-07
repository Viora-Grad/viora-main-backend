using Viora.Infrastructure;
using global::Viora.Infrastructure.Repositories.RealTimeScheduling;

namespace Viora.Test.Compenents.Infrastructure.RealTimeScheduling;

/// <summary>
/// Unit tests for the ScheduleDelayRepository against an InMemory database.
/// ScheduleDelay has InitiatorType (record = ComplexProperty) which InMemory cannot query-back.
/// Tests verify Add and empty-result assertions only.
/// </summary>
[TestClass]
public sealed class ScheduleDelayRepositoryTests : InfrastructureTestBase
{
    private readonly ScheduleDelayRepository _repository;

    public ScheduleDelayRepositoryTests()
    {
        _repository = new ScheduleDelayRepository(DbContext);
    }

    // ===== Add =====

    [TestMethod]
    public async Task Add_Delay_PersistsToDatabase()
    {
        var delay = global::Viora.Domain.RealTimeScheduling.ScheduleDelay.Create(
            Guid.NewGuid(), new TimeOnly(0, 30), "Traffic jam", DateTime.UtcNow, global::Viora.Domain.RealTimeScheduling.Internals.InitiatorType.Client);

        _repository.Add(delay);
        await DbContext.SaveChangesAsync();

        Assert.IsTrue(delay.Id != Guid.Empty);
    }

    // ===== AddRange =====

    [TestMethod]
    public async Task AddRange_MultipleDelays_PersistsAll()
    {
        var delays = new[]
        {
            global::Viora.Domain.RealTimeScheduling.ScheduleDelay.Create(
                Guid.NewGuid(), new TimeOnly(0, 15), "Late start", DateTime.UtcNow, global::Viora.Domain.RealTimeScheduling.Internals.InitiatorType.Client),
            global::Viora.Domain.RealTimeScheduling.ScheduleDelay.Create(
                Guid.NewGuid(), new TimeOnly(0, 45), "System issue", DateTime.UtcNow, global::Viora.Domain.RealTimeScheduling.Internals.InitiatorType.System),
        };

        _repository.AddRange(delays);
        await DbContext.SaveChangesAsync();

        foreach (var delay in delays)
        {
            Assert.IsTrue(delay.Id != Guid.Empty);
        }
    }

    // ===== GetByIdAsync (empty result only) =====

    [TestMethod]
    public async Task GetByIdAsync_DelayNotFound_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.IsNull(result);
    }
}
