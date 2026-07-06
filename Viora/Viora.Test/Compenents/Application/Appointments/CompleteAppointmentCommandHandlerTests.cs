using Moq;
using System.Reflection;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Appointments.CompleteAppointment;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;
using Viora.Domain.Staffs;

namespace Viora.Test.Compenents.Application.Appointments;

[TestClass]
public sealed class CompleteAppointmentCommandHandlerTests
{
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IStaffRepository> _staffRepoMock = new();
    private readonly Mock<IAppointmentsRepository> _appointmentRepoMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly DateTime _fixedNow = new(2026, 7, 6, 10, 30, 0, DateTimeKind.Utc);
    private static readonly Guid BranchId = Guid.NewGuid();
    private static readonly Guid OrgId = Guid.NewGuid();
    private readonly CompleteAppointmentCommandHandler _handler;

    public CompleteAppointmentCommandHandlerTests()
    {
        _clockMock.Setup(c => c.UtcNow).Returns(_fixedNow);
        _handler = new CompleteAppointmentCommandHandler(
            _userContextMock.Object,
            _staffRepoMock.Object,
            _appointmentRepoMock.Object,
            _clockMock.Object,
            _unitOfWorkMock.Object);
    }

    private Appointment CreateInProgressAppointment(DateTime? reservationDate = null)
    {
        var resDate = reservationDate ?? _fixedNow.AddMinutes(-25);
        var appointment = Appointment.Book(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), BranchId, null,
            resDate, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, _fixedNow.AddDays(-1));
        appointment.CheckIn(resDate.AddMinutes(-15), Creator.Customer);
        return appointment;
    }

    /// <summary>
    /// Creates an Appointment with the Staff navigation property set via reflection.
    /// The handler accesses appointment.Staff.OrganizationId for org authorization.
    /// </summary>
    private Appointment CreateInProgressAppointmentWithStaff(Guid orgId, DateTime? reservationDate = null)
    {
        var appointment = CreateInProgressAppointment(reservationDate);
        var staff = Staff.Create(orgId, _fixedNow);
        var prop = typeof(Appointment).GetProperty("Staff",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        prop!.SetValue(appointment, staff);
        return appointment;
    }

    [TestMethod]
    public async Task Handle_ValidComplete_ReturnsSuccess()
    {
        Appointment appointment = CreateInProgressAppointmentWithStaff(OrgId, _fixedNow.AddMinutes(-60));
        var command = new CompleteAppointmentCommand(appointment.Id);

        _userContextMock.Setup(c => c.OrganizationId).Returns(OrgId);
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _appointmentRepoMock.Setup(r => r.GetByDateRangeAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<IEnumerable<Appointment>>(new List<Appointment>()));

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(CustomerStatus.Completed, appointment.Status);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_AppointmentNotFound_ThrowsNotFoundException()
    {
        var command = new CompleteAppointmentCommand(Guid.NewGuid());

        _userContextMock.Setup(c => c.OrganizationId).Returns(OrgId);
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_DifferentOrg_ThrowsUnauthorizedAccessException()
    {
        Appointment appointment = CreateInProgressAppointmentWithStaff(OrgId);
        var command = new CompleteAppointmentCommand(appointment.Id);

        _userContextMock.Setup(c => c.OrganizationId).Returns(Guid.NewGuid());
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_AlreadyCompleted_ReturnsFailure()
    {
        Appointment appointment = CreateInProgressAppointmentWithStaff(OrgId);
        appointment.Complete(_fixedNow);
        var command = new CompleteAppointmentCommand(appointment.Id);

        _userContextMock.Setup(c => c.OrganizationId).Returns(OrgId);
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.CompleteProhibited, result.Error);
    }
}
