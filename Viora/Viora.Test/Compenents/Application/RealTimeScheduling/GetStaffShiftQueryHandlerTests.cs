using Moq;
using System.Reflection;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.RealTimeScheduling.GetStaffShiftQuery;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.RealTimeScheduling;
using Viora.Domain.Services;
using Viora.Domain.Staffs;

namespace Viora.Test.Compenents.Application.RealTimeScheduling;

/// <summary>
/// Unit tests for the GetStaffShiftQeuryHandler covering successful retrieval, not-found, and no shifts scenarios.
/// </summary>
[TestClass]
public sealed class GetStaffShiftQueryHandlerTests
{
    private readonly Mock<IBranchRepository> _branchRepoMock = new();
    private readonly Mock<IStaffRepository> _staffRepoMock = new();
    private readonly Mock<IScheduleRepository> _scheduleRepoMock = new();
    private readonly GetStaffShiftQeuryHandler _handler;

    public GetStaffShiftQueryHandlerTests()
    {
        _handler = new GetStaffShiftQeuryHandler(
            _branchRepoMock.Object,
            _staffRepoMock.Object,
            _scheduleRepoMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_StaffNotFound_ThrowsNotFoundException()
    {
        Guid staffId = Guid.NewGuid();
        _staffRepoMock.Setup(r => r.GetByIdAsync(staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Staff?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetStaffShiftQuery(staffId, Guid.NewGuid()), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_BranchNotFound_ThrowsNotFoundException()
    {
        var staff = CreateTestStaff();
        _staffRepoMock.Setup(r => r.GetByIdAsync(staff.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _branchRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetStaffShiftQuery(staff.Id, Guid.NewGuid()), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_StaffHasShifts_ReturnsShifts()
    {
        var staff = CreateTestStaff();
        var branch = CreateTestBranch();
        var schedule = CreateTestScheduleWithStaffShifts(branch.Id, staff.Id, DayOfWeek.Monday);

        _staffRepoMock.Setup(r => r.GetByIdAsync(staff.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _branchRepoMock.Setup(r => r.GetByIdAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _scheduleRepoMock.Setup(r => r.getByBranchIdAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Schedule> { schedule });

        var result = await _handler.Handle(
            new GetStaffShiftQuery(staff.Id, branch.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Value.Count);
        Assert.AreEqual(staff.Id, result.Value[0].StaffId);
        Assert.AreEqual("Monday", result.Value[0].Day);
    }

    [TestMethod]
    public async Task Handle_StaffHasNoShifts_ReturnsFailure()
    {
        var staff = CreateTestStaff();
        var branch = CreateTestBranch();
        var schedule = CreateTestScheduleWithStaffShifts(branch.Id, Guid.NewGuid(), DayOfWeek.Monday);

        _staffRepoMock.Setup(r => r.GetByIdAsync(staff.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _branchRepoMock.Setup(r => r.GetByIdAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _scheduleRepoMock.Setup(r => r.getByBranchIdAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Schedule> { schedule });

        var result = await _handler.Handle(
            new GetStaffShiftQuery(staff.Id, branch.Id), CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(ScheduleError.ShiftsNotFound.Name, result.Error.Name);
    }

    // ===== Helpers =====

    private static Staff CreateTestStaff()
    {
        return Staff.Create(Guid.NewGuid(), DateTime.UtcNow);
    }

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

    private static Schedule CreateTestScheduleWithStaffShifts(Guid branchId, Guid staffId, DayOfWeek day)
    {
        var schedule = Schedule.Create(branchId, day);
        var shift = Shift.Create(schedule.Id, new TimeOnly(9, 0), new TimeOnly(17, 0), staffId);
        var intervals = new List<Shift> { shift };
        typeof(Schedule).GetField("_intervals", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(schedule, intervals);
        return schedule;
    }
}
