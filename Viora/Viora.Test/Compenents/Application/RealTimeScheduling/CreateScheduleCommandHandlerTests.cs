using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.RealTimeScheduling.CreateSchedule;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.RealTimeScheduling;

namespace Viora.Test.Compenents.Application.RealTimeScheduling;

/// <summary>
/// Unit tests for the CreateScheduleCommandHandler covering successful creation, branch not found, and schedule overlap scenarios.
/// </summary>
[TestClass]
public sealed class CreateScheduleCommandHandlerTests
{
    private readonly Mock<IBranchRepository> _branchRepoMock = new();
    private readonly Mock<IScheduleRepository> _scheduleRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CreateScheduleCommandHandler _handler;

    public CreateScheduleCommandHandlerTests()
    {
        _handler = new CreateScheduleCommandHandler(
            _branchRepoMock.Object,
            _scheduleRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_BranchNotFound_ThrowsNotFoundException()
    {
        Guid branchId = Guid.NewGuid();
        _branchRepoMock.Setup(r => r.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new CreateScheduleCommand(branchId, "Monday"), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_ScheduleAlreadyExistsForDay_ReturnsFailure()
    {
        var branch = CreateTestBranch();
        var schedule = Schedule.Create(branch.Id, DayOfWeek.Monday);

        _branchRepoMock.Setup(r => r.GetByIdAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _scheduleRepoMock.Setup(r => r.getByBranchIdAndDayAsync(branch.Id, DayOfWeek.Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        var result = await _handler.Handle(
            new CreateScheduleCommand(branch.Id, "Monday"), CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(ScheduleError.ScheduleOverLap.Name, result.Error.Name);
    }

    [TestMethod]
    public async Task Handle_ValidSchedule_CreatesSchedule()
    {
        var branch = CreateTestBranch();

        _branchRepoMock.Setup(r => r.GetByIdAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _scheduleRepoMock.Setup(r => r.getByBranchIdAndDayAsync(branch.Id, DayOfWeek.Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Schedule?)null);
        _unitOfWorkMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(
            new CreateScheduleCommand(branch.Id, "Monday"), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreNotEqual(Guid.Empty, result.Value);
        _scheduleRepoMock.Verify(r => r.Add(It.IsAny<Schedule>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_ValidSchedule_SavesChanges()
    {
        var branch = CreateTestBranch();

        _branchRepoMock.Setup(r => r.GetByIdAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _scheduleRepoMock.Setup(r => r.getByBranchIdAndDayAsync(branch.Id, DayOfWeek.Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Schedule?)null);
        _unitOfWorkMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(new CreateScheduleCommand(branch.Id, "Monday"), CancellationToken.None);

        _unitOfWorkMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
}
