using Viora.Infrastructure;
using global::Viora.Infrastructure.Repositories.RealTimeScheduling;

namespace Viora.Test.Compenents.Infrastructure.RealTimeScheduling;

/// <summary>
/// Unit tests for the ShiftRepository against an InMemory database.
/// Shift has no ComplexProperty so queries work normally.
/// Note: base Remove uses ExecuteDelete which InMemory does not support.
/// </summary>
[TestClass]
public sealed class ShiftRepositoryTests : InfrastructureTestBase
{
    private readonly ShiftRepository _repository;

    public ShiftRepositoryTests()
    {
        _repository = new ShiftRepository(DbContext);
    }

    // ===== Add =====

    [TestMethod]
    public async Task Add_Shift_PersistsToDatabase()
    {
        var shift = global::Viora.Domain.RealTimeScheduling.Shift.Create(
            Guid.NewGuid(), new TimeOnly(9, 0), new TimeOnly(17, 0), Guid.NewGuid());

        _repository.Add(shift);
        await DbContext.SaveChangesAsync();

        Assert.IsTrue(shift.Id != Guid.Empty);
    }

    // ===== GetByIdAsync =====

    [TestMethod]
    public async Task GetByIdAsync_ShiftExists_ReturnsShift()
    {
        var shift = global::Viora.Domain.RealTimeScheduling.Shift.Create(
            Guid.NewGuid(), new TimeOnly(9, 0), new TimeOnly(17, 0), Guid.NewGuid());
        DbContext.Set<global::Viora.Domain.RealTimeScheduling.Shift>().Add(shift);
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(shift.Id, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(shift.Id, result.Id);
        Assert.AreEqual(shift.StartTime, result.StartTime);
        Assert.AreEqual(shift.EndTime, result.EndTime);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShiftNotFound_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.IsNull(result);
    }

    // ===== GetActiveShiftAsync =====

    [TestMethod]
    public async Task GetActiveShiftAsync_ShiftExists_ReturnsShift()
    {
        var scheduleId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        var shift = global::Viora.Domain.RealTimeScheduling.Shift.Create(
            scheduleId, new TimeOnly(9, 0), new TimeOnly(17, 0), staffId);
        DbContext.Set<global::Viora.Domain.RealTimeScheduling.Shift>().Add(shift);
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetActiveShiftAsync(scheduleId, staffId, new TimeOnly(12, 0), CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(scheduleId, result.ScheduleId);
        Assert.AreEqual(staffId, result.StaffId);
    }

    [TestMethod]
    public async Task GetActiveShiftAsync_TimeOutsideShift_ReturnsNull()
    {
        var scheduleId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        var shift = global::Viora.Domain.RealTimeScheduling.Shift.Create(
            scheduleId, new TimeOnly(9, 0), new TimeOnly(17, 0), staffId);
        DbContext.Set<global::Viora.Domain.RealTimeScheduling.Shift>().Add(shift);
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetActiveShiftAsync(scheduleId, staffId, new TimeOnly(20, 0), CancellationToken.None);
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetActiveShiftAsync_DifferentSchedule_ReturnsNull()
    {
        var staffId = Guid.NewGuid();
        var shift = global::Viora.Domain.RealTimeScheduling.Shift.Create(
            Guid.NewGuid(), new TimeOnly(9, 0), new TimeOnly(17, 0), staffId);
        DbContext.Set<global::Viora.Domain.RealTimeScheduling.Shift>().Add(shift);
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetActiveShiftAsync(Guid.NewGuid(), staffId, new TimeOnly(12, 0), CancellationToken.None);
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetActiveShiftAsync_DifferentStaff_ReturnsNull()
    {
        var scheduleId = Guid.NewGuid();
        var shift = global::Viora.Domain.RealTimeScheduling.Shift.Create(
            scheduleId, new TimeOnly(9, 0), new TimeOnly(17, 0), Guid.NewGuid());
        DbContext.Set<global::Viora.Domain.RealTimeScheduling.Shift>().Add(shift);
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetActiveShiftAsync(scheduleId, Guid.NewGuid(), new TimeOnly(12, 0), CancellationToken.None);
        Assert.IsNull(result);
    }

    // ===== Remove (via DbContext directly since ExecuteDelete not supported by InMemory) =====

    [TestMethod]
    public async Task Remove_Shift_DeletesFromDatabase()
    {
        var shift = global::Viora.Domain.RealTimeScheduling.Shift.Create(
            Guid.NewGuid(), new TimeOnly(9, 0), new TimeOnly(17, 0), Guid.NewGuid());
        DbContext.Set<global::Viora.Domain.RealTimeScheduling.Shift>().Add(shift);
        await DbContext.SaveChangesAsync();

        DbContext.Set<global::Viora.Domain.RealTimeScheduling.Shift>().Remove(shift);
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(shift.Id, CancellationToken.None);
        Assert.IsNull(result);
    }
}
