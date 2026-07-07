using Moq;
using System.Reflection;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.RealTimeScheduling.CreateRecurringSchedule;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.RealTimeScheduling;
using Viora.Domain.Services;
using Viora.Domain.Staffs;

namespace Viora.Test.Compenents.Application.RealTimeScheduling;

/// <summary>
/// Unit tests for the CreateShiftCommandHandler covering successful creation, branch/staff not found, schedule not found, and overlap scenarios.
/// </summary>
[TestClass]
public sealed class CreateShiftCommandHandlerTests
{
    private readonly Mock<IScheduleRepository> _scheduleRepoMock = new();
    private readonly Mock<IBranchRepository> _branchRepoMock = new();
    private readonly Mock<IShiftRepository> _shiftRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IStaffRepository> _staffRepoMock = new();
    private readonly CreateShiftCommandHandler _handler;

    public CreateShiftCommandHandlerTests()
    {
        _handler = new CreateShiftCommandHandler(
            _scheduleRepoMock.Object,
            _branchRepoMock.Object,
            _shiftRepoMock.Object,
            _unitOfWorkMock.Object,
            _staffRepoMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_BranchNotFound_ThrowsNotFoundException()
    {
        Guid branchId = Guid.NewGuid();
        _branchRepoMock.Setup(r => r.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new CreateShiftCommand(branchId, new TimeOnly(9, 0), new TimeOnly(17, 0), "Monday", Guid.NewGuid()), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_StaffNotFound_ThrowsNotFoundException()
    {
        Guid staffId = Guid.NewGuid();
        var branch = CreateTestBranch();
        _branchRepoMock.Setup(r => r.GetByIdAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _staffRepoMock.Setup(r => r.GetByIdAsync(staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Staff?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new CreateShiftCommand(branch.Id, new TimeOnly(9, 0), new TimeOnly(17, 0), "Monday", staffId), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_NoScheduleForDay_ReturnsFailure()
    {
        var branch = CreateTestBranch();
        Guid staffId = Guid.NewGuid();
        _branchRepoMock.Setup(r => r.GetByIdAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _staffRepoMock.Setup(r => r.GetByIdAsync(staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestStaff(staffId));
        _scheduleRepoMock.Setup(r => r.getByBranchIdAndDayAsync(branch.Id, DayOfWeek.Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Schedule?)null);

        var result = await _handler.Handle(
            new CreateShiftCommand(branch.Id, new TimeOnly(9, 0), new TimeOnly(17, 0), "Monday", staffId), CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(ScheduleError.NotFoundForDay.Name, result.Error.Name);
    }

    [TestMethod]
    public async Task Handle_ShiftOverlap_ReturnsFailure()
    {
        var branch = CreateTestBranch();
        Guid staffId = Guid.NewGuid();
        var schedule = Schedule.Create(branch.Id, DayOfWeek.Monday);
        var existingShift = Shift.Create(schedule.Id, new TimeOnly(9, 0), new TimeOnly(17, 0), staffId);
        SetScheduleIntervals(schedule, new List<Shift> { existingShift });

        _branchRepoMock.Setup(r => r.GetByIdAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _staffRepoMock.Setup(r => r.GetByIdAsync(staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestStaff(staffId));
        _scheduleRepoMock.Setup(r => r.getByBranchIdAndDayAsync(branch.Id, DayOfWeek.Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        var result = await _handler.Handle(
            new CreateShiftCommand(branch.Id, new TimeOnly(9, 0), new TimeOnly(17, 0), "Monday", staffId), CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(ScheduleError.ShiftOverlap.Name, result.Error.Name);
    }

    [TestMethod]
    public async Task Handle_ValidShift_CreatesShift()
    {
        var branch = CreateTestBranch();
        Guid staffId = Guid.NewGuid();
        var schedule = Schedule.Create(branch.Id, DayOfWeek.Monday);
        SetScheduleIntervals(schedule, new List<Shift>());

        _branchRepoMock.Setup(r => r.GetByIdAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _staffRepoMock.Setup(r => r.GetByIdAsync(staffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestStaff(staffId));
        _scheduleRepoMock.Setup(r => r.getByBranchIdAndDayAsync(branch.Id, DayOfWeek.Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        _unitOfWorkMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(
            new CreateShiftCommand(branch.Id, new TimeOnly(9, 0), new TimeOnly(17, 0), "Monday", staffId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreNotEqual(Guid.Empty, result.Value);
        _shiftRepoMock.Verify(r => r.Add(It.IsAny<Shift>()), Times.Once);
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

    private static Staff CreateTestStaff(Guid id)
    {
        return Staff.Create(Guid.NewGuid(), DateTime.UtcNow, id);
    }

    private static void SetScheduleIntervals(Schedule schedule, List<Shift> intervals)
    {
        typeof(Schedule).GetField("_intervals", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(schedule, intervals);
    }
}
