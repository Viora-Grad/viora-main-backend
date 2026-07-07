using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.RealTimeScheduling.GetBranchSchedule;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.RealTimeScheduling;
using Viora.Domain.Services;
using System.Reflection;

namespace Viora.Test.Compenents.Application.RealTimeScheduling;

/// <summary>
/// Unit tests for the GetBranchScheduleQueryHandler covering successful retrieval, branch not found, and no schedules scenarios.
/// </summary>
[TestClass]
public sealed class GetBranchScheduleQueryHandlerTests
{
    private readonly Mock<IBranchRepository> _branchRepoMock = new();
    private readonly Mock<IScheduleRepository> _scheduleRepoMock = new();
    private readonly GetBranchScheduleQueryHandler _handler;

    public GetBranchScheduleQueryHandlerTests()
    {
        _handler = new GetBranchScheduleQueryHandler(
            _branchRepoMock.Object,
            _scheduleRepoMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_BranchNotFound_ThrowsNotFoundException()
    {
        Guid branchId = Guid.NewGuid();
        _branchRepoMock.Setup(r => r.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetBranchScheduleQuery(branchId), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_NoSchedules_ReturnsFailure()
    {
        var branch = CreateTestBranch();
        _branchRepoMock.Setup(r => r.GetByIdAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _scheduleRepoMock.Setup(r => r.getByBranchIdAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Schedule>());

        var result = await _handler.Handle(
            new GetBranchScheduleQuery(branch.Id), CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(ScheduleError.ScheduleOverLap.Name, result.Error.Name);
    }

    [TestMethod]
    public async Task Handle_SchedulesExist_ReturnsMappedResponse()
    {
        var branch = CreateTestBranch();
        var schedule1 = CreateTestScheduleWithShifts(branch.Id, DayOfWeek.Monday);
        var schedule2 = CreateTestScheduleWithShifts(branch.Id, DayOfWeek.Tuesday);

        _branchRepoMock.Setup(r => r.GetByIdAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _scheduleRepoMock.Setup(r => r.getByBranchIdAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Schedule> { schedule1, schedule2 });

        var result = await _handler.Handle(
            new GetBranchScheduleQuery(branch.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, result.Value.Count);
    }

    [TestMethod]
    public async Task Handle_ScheduleWithShifts_ReturnsShiftsInResponse()
    {
        var branch = CreateTestBranch();
        var schedule = CreateTestScheduleWithShifts(branch.Id, DayOfWeek.Monday);

        _branchRepoMock.Setup(r => r.GetByIdAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _scheduleRepoMock.Setup(r => r.getByBranchIdAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Schedule> { schedule });

        var result = await _handler.Handle(
            new GetBranchScheduleQuery(branch.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Value[0].Shifts.Count);
        Assert.AreEqual("Monday", result.Value[0].Day);
    }

    // ===== Helpers =====

    private static Branch CreateTestBranch()
    {
        return Branch.Create(
            Guid.NewGuid(),
            new Viora.Domain.Shared.Internal.Address(1, "123 St", "City", "State", Guid.NewGuid(), 12345),
            new NetTopologySuite.Geometries.Point(0, 0) { SRID = 4326 },
            "test@example.com",
            new List<Viora.Domain.Shared.ServiceType> { Viora.Domain.Shared.ServiceType.InternalMedicine },
            DateTime.UtcNow).Value;
    }

    private static Schedule CreateTestScheduleWithShifts(Guid branchId, DayOfWeek day)
    {
        var schedule = Schedule.Create(branchId, day);
        var shift = Shift.Create(schedule.Id, new TimeOnly(9, 0), new TimeOnly(17, 0), Guid.NewGuid());
        var intervals = new List<Shift> { shift };
        typeof(Schedule).GetField("_intervals", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(schedule, intervals);
        return schedule;
    }
}
