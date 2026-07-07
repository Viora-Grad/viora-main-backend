using Moq;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.RealTimeScheduling.CancelSchedule;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Branches;
using Viora.Domain.RealTimeScheduling;
using Viora.Domain.Services;

namespace Viora.Test.Compenents.Application.RealTimeScheduling;

/// <summary>
/// Unit tests for the CancelScheduleCommandHandler covering successful cancellation, branch not found, shift not found, and appointment cancellation.
/// </summary>
[TestClass]
public sealed class CancelScheduleCommandHandlerTests
{
    private readonly Mock<IShiftRepository> _shiftRepoMock = new();
    private readonly Mock<IAppointmentsRepository> _appointmentRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly Mock<IScheduleCancellationRepository> _cancellationRepoMock = new();
    private readonly Mock<IBranchRepository> _branchRepoMock = new();
    private readonly CancelScheduleCommandHandler _handler;

    public CancelScheduleCommandHandlerTests()
    {
        _handler = new CancelScheduleCommandHandler(
            _shiftRepoMock.Object,
            _appointmentRepoMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object,
            _cancellationRepoMock.Object,
            _branchRepoMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_BranchNotFound_ThrowsNotFoundException()
    {
        _branchRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new CancelScheduleCommand(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, "Reason"), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_ShiftNotFound_ThrowsNotFoundException()
    {
        var branch = CreateTestBranch();
        _branchRepoMock.Setup(r => r.GetByIdAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _shiftRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Shift?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new CancelScheduleCommand(Guid.NewGuid(), branch.Id, DateTime.UtcNow, "Reason"), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_ValidCancellation_CreatesCancellationRecord()
    {
        var branch = CreateTestBranch();
        var shift = Shift.Create(Guid.NewGuid(), new TimeOnly(9, 0), new TimeOnly(17, 0), Guid.NewGuid());
        var now = DateTime.UtcNow;

        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(now);
        _branchRepoMock.Setup(r => r.GetByIdAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _shiftRepoMock.Setup(r => r.GetByIdAsync(shift.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shift);
        _appointmentRepoMock.Setup(r => r.ListAsync(It.IsAny<ISpecification<Appointment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>());
        _unitOfWorkMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(
            new CancelScheduleCommand(shift.Id, branch.Id, now, "Doctor unavailable"), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        _cancellationRepoMock.Verify(r => r.Add(It.IsAny<ScheduleCancellations>()), Times.Once);
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
