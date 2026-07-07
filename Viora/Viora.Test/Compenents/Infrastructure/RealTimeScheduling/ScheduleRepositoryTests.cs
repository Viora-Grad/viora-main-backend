using Viora.Infrastructure;
using global::Viora.Infrastructure.Repositories.RealTimeScheduling;

namespace Viora.Test.Compenents.Infrastructure.RealTimeScheduling;

/// <summary>
/// Unit tests for the ScheduleRepository against an InMemory database.
/// </summary>
[TestClass]
public sealed class ScheduleRepositoryTests : InfrastructureTestBase
{
    private readonly ScheduleRepository _repository;

    public ScheduleRepositoryTests()
    {
        _repository = new ScheduleRepository(DbContext);
    }

    // ===== Add =====

    [TestMethod]
    public async Task Add_Schedule_PersistsToDatabase()
    {
        var schedule = global::Viora.Domain.RealTimeScheduling.Schedule.Create(Guid.NewGuid(), DayOfWeek.Monday);

        _repository.Add(schedule);
        await DbContext.SaveChangesAsync();

        Assert.IsTrue(schedule.Id != Guid.Empty);
    }

    // ===== GetByIdAsync =====

    [TestMethod]
    public async Task GetByIdAsync_ScheduleExists_ReturnsSchedule()
    {
        var schedule = global::Viora.Domain.RealTimeScheduling.Schedule.Create(Guid.NewGuid(), DayOfWeek.Monday);
        DbContext.Set<global::Viora.Domain.RealTimeScheduling.Schedule>().Add(schedule);
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(schedule.Id, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(schedule.Id, result.Id);
        Assert.AreEqual(DayOfWeek.Monday, result.DayOfWeek);
    }

    [TestMethod]
    public async Task GetByIdAsync_ScheduleNotFound_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.IsNull(result);
    }

    // ===== getByBranchIdAndDayAsync =====

    [TestMethod]
    public async Task GetByBranchIdAndDayAsync_ScheduleExists_ReturnsWithIntervals()
    {
        var branchId = Guid.NewGuid();
        var schedule = global::Viora.Domain.RealTimeScheduling.Schedule.Create(branchId, DayOfWeek.Monday);
        DbContext.Set<global::Viora.Domain.RealTimeScheduling.Schedule>().Add(schedule);
        await DbContext.SaveChangesAsync();

        var result = await _repository.getByBranchIdAndDayAsync(branchId, DayOfWeek.Monday, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(branchId, result.BranchId);
        Assert.AreEqual(DayOfWeek.Monday, result.DayOfWeek);
    }

    [TestMethod]
    public async Task GetByBranchIdAndDayAsync_ScheduleNotFound_ReturnsNull()
    {
        var result = await _repository.getByBranchIdAndDayAsync(Guid.NewGuid(), DayOfWeek.Monday, CancellationToken.None);
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetByBranchIdAndDayAsync_WrongDay_ReturnsNull()
    {
        var branchId = Guid.NewGuid();
        var schedule = global::Viora.Domain.RealTimeScheduling.Schedule.Create(branchId, DayOfWeek.Monday);
        DbContext.Set<global::Viora.Domain.RealTimeScheduling.Schedule>().Add(schedule);
        await DbContext.SaveChangesAsync();

        var result = await _repository.getByBranchIdAndDayAsync(branchId, DayOfWeek.Friday, CancellationToken.None);
        Assert.IsNull(result);
    }

    // ===== getByBranchIdAsync =====

    [TestMethod]
    public async Task GetByBranchIdAsync_BranchWithSchedules_ReturnsAll()
    {
        var branchId = Guid.NewGuid();
        var schedule1 = global::Viora.Domain.RealTimeScheduling.Schedule.Create(branchId, DayOfWeek.Monday);
        var schedule2 = global::Viora.Domain.RealTimeScheduling.Schedule.Create(branchId, DayOfWeek.Tuesday);

        DbContext.Set<global::Viora.Domain.RealTimeScheduling.Schedule>().AddRange(schedule1, schedule2);
        await DbContext.SaveChangesAsync();

        var result = await _repository.getByBranchIdAsync(branchId, CancellationToken.None);

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetByBranchIdAsync_BranchWithNoSchedules_ReturnsEmpty()
    {
        var result = await _repository.getByBranchIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetByBranchIdAsync_DifferentBranch_ReturnsEmpty()
    {
        var schedule = global::Viora.Domain.RealTimeScheduling.Schedule.Create(Guid.NewGuid(), DayOfWeek.Monday);
        DbContext.Set<global::Viora.Domain.RealTimeScheduling.Schedule>().Add(schedule);
        await DbContext.SaveChangesAsync();

        var result = await _repository.getByBranchIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.AreEqual(0, result.Count);
    }

    // ===== Helpers =====

    // Schedule uses primitive types, no helper needed.
}
