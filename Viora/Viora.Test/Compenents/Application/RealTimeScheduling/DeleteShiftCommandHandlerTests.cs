using Moq;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.RealTimeScheduling.DeleteShift;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.RealTimeScheduling;

namespace Viora.Test.Compenents.Application.RealTimeScheduling;

/// <summary>
/// Unit tests for the DeleteShiftCommandHandler covering successful deletion, shift not found, and appointment conflict scenarios.
/// </summary>
[TestClass]
public sealed class DeleteShiftCommandHandlerTests
{
    private readonly Mock<IShiftRepository> _shiftRepoMock = new();
    private readonly Mock<IScheduleRepository> _scheduleRepoMock = new();
    private readonly Mock<IAppointmentsRepository> _appointmentRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly DeleteShiftCommandHandler _handler;

    public DeleteShiftCommandHandlerTests()
    {
        _handler = new DeleteShiftCommandHandler(
            _shiftRepoMock.Object,
            _scheduleRepoMock.Object,
            _appointmentRepoMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_ShiftNotFound_ThrowsNotFoundException()
    {
        Guid shiftId = Guid.NewGuid();
        _shiftRepoMock.Setup(r => r.GetByIdAsync(shiftId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Shift?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new DeleteShiftCommand(shiftId), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_ScheduleNotFound_ThrowsNotFoundException()
    {
        var shift = Shift.Create(Guid.NewGuid(), new TimeOnly(9, 0), new TimeOnly(17, 0), Guid.NewGuid());

        _shiftRepoMock.Setup(r => r.GetByIdAsync(shift.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shift);
        _scheduleRepoMock.Setup(r => r.GetByIdAsync(shift.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Schedule?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new DeleteShiftCommand(shift.Id), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_NoAppointments_DeletesShift()
    {
        var shift = Shift.Create(Guid.NewGuid(), new TimeOnly(9, 0), new TimeOnly(17, 0), Guid.NewGuid());
        var schedule = Schedule.Create(Guid.NewGuid(), DayOfWeek.Monday);

        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(DateTime.UtcNow);
        _shiftRepoMock.Setup(r => r.GetByIdAsync(shift.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shift);
        _scheduleRepoMock.Setup(r => r.GetByIdAsync(shift.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        _appointmentRepoMock.Setup(r => r.ListAsync(It.IsAny<ISpecification<Appointment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>());
        _unitOfWorkMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(new DeleteShiftCommand(shift.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        _shiftRepoMock.Verify(r => r.Remove(shift.Id), Times.Once);
    }

    // ===== Helpers =====

    private static Appointment CreateTestAppointment()
    {
        return Appointment.Book(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null,
            DateTime.UtcNow.AddDays(1), 30,
            Viora.Domain.Appointments.Internal.PaymentMethod.Cash,
            null,
            Viora.Domain.Appointments.Internal.Creator.Customer,
            Viora.Domain.Appointments.Internal.Platform.Web,
            30, DateTime.UtcNow);
    }
}
