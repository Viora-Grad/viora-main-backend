using Moq;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Scheduling;
using Viora.Application.Reminders.CreateReminder;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;
using Viora.Domain.Reminders;
using Viora.Domain.Reminders.Events;

namespace Viora.Test.Compenents.Application.Reminders;

[TestClass]
public sealed class CreateReminderCommandHandlerTests
{
    private readonly Mock<IAppointmentsRepository> _appointmentRepoMock = new();
    private readonly Mock<IReminderRepository> _reminderRepoMock = new();
    private readonly Mock<IDomainEventScheduler> _schedulerMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly DateTime _fixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);
    private readonly CreateReminderCommandHandler _handler;

    public CreateReminderCommandHandlerTests()
    {
        _clockMock.Setup(c => c.UtcNow).Returns(_fixedNow);
        _handler = new CreateReminderCommandHandler(
            _appointmentRepoMock.Object,
            _reminderRepoMock.Object,
            _schedulerMock.Object,
            _clockMock.Object,
            _unitOfWorkMock.Object);
    }

    [TestMethod]
    public async Task Handle_CompletedAppointment_ReturnsReminderId()
    {
        var appointmentId = Guid.NewGuid();
        var appointment = Appointment.Book(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null,
            _fixedNow.AddDays(-1), 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, _fixedNow.AddDays(-2));
        appointment.CheckIn(_fixedNow.AddDays(-1).AddMinutes(-15), Creator.Customer);
        appointment.Complete(_fixedNow.AddDays(-1).AddMinutes(25));

        var command = new CreateReminderCommand(
            appointmentId, "Follow-up", "Please come back", _fixedNow.AddDays(7));

        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _schedulerMock.Setup(s => s.ScheduleAsync(
            It.IsAny<ReminderCreatedEvent>(),
            command.ScheduledFor,
            It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Guid.NewGuid()));

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreNotEqual(Guid.Empty, result.Value);
        _reminderRepoMock.Verify(r => r.Add(It.IsAny<Reminder>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_AppointmentNotFound_ThrowsNotFoundException()
    {
        var command = new CreateReminderCommand(
            Guid.NewGuid(), "Title", null, _fixedNow.AddDays(1));

        _appointmentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_AppointmentNotCompleted_ThrowsConflictException()
    {
        var appointmentId = Guid.NewGuid();
        var appointment = Appointment.Book(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null,
            _fixedNow.AddDays(-1), 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, _fixedNow.AddDays(-2));

        var command = new CreateReminderCommand(
            appointmentId, "Title", null, _fixedNow.AddDays(1));

        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
