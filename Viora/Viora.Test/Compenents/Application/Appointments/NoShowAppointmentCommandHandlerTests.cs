using Moq;
using System.Reflection;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Appointments.NoShowAppointment;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;
using Viora.Domain.Staffs;

namespace Viora.Test.Compenents.Application.Appointments;

[TestClass]
public sealed class NoShowAppointmentCommandHandlerTests
{
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IStaffRepository> _staffRepoMock = new();
    private readonly Mock<IAppointmentsRepository> _appointmentRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();

    private readonly DateTime _fixedNow = new(2026, 7, 6, 11, 0, 0, DateTimeKind.Utc);
    private static readonly Guid BranchId = Guid.NewGuid();
    private static readonly Guid OrgId = Guid.NewGuid();
    private readonly NoShowAppointmentCommandHandler _handler;

    public NoShowAppointmentCommandHandlerTests()
    {
        _clockMock.Setup(c => c.UtcNow).Returns(_fixedNow);
        _handler = new NoShowAppointmentCommandHandler(
            _userContextMock.Object,
            _staffRepoMock.Object,
            _appointmentRepoMock.Object,
            _unitOfWorkMock.Object,
            _clockMock.Object);
    }

    private Appointment CreateTestAppointment(DateTime? reservationDate = null)
    {
        var resDate = reservationDate ?? _fixedNow.AddHours(-1);
        return Appointment.Book(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), BranchId, null,
            resDate, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, _fixedNow.AddDays(-1));
    }

    /// <summary>
    /// Creates an Appointment with the Staff navigation property set via reflection.
    /// The handler accesses appointment.Staff.OrganizationId for org authorization.
    /// </summary>
    private Appointment CreateTestAppointmentWithStaff(Guid orgId, DateTime? reservationDate = null)
    {
        var appointment = CreateTestAppointment(reservationDate);
        var staff = Staff.Create(orgId, _fixedNow);
        var prop = typeof(Appointment).GetProperty("Staff",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        prop!.SetValue(appointment, staff);
        return appointment;
    }

    [TestMethod]
    public async Task Handle_ValidNoShow_ReturnsSuccess()
    {
        Appointment appointment = CreateTestAppointmentWithStaff(OrgId);
        var command = new NoShowAppointmentCommand(appointment.Id);

        _userContextMock.Setup(c => c.OrganizationId).Returns(OrgId);
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(CustomerStatus.NoShow, appointment.Status);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_AppointmentNotFound_ThrowsNotFoundException()
    {
        var command = new NoShowAppointmentCommand(Guid.NewGuid());

        _userContextMock.Setup(c => c.OrganizationId).Returns(OrgId);
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_DifferentOrg_ThrowsUnauthorizedAccessException()
    {
        Appointment appointment = CreateTestAppointmentWithStaff(OrgId);
        var command = new NoShowAppointmentCommand(appointment.Id);

        _userContextMock.Setup(c => c.OrganizationId).Returns(Guid.NewGuid());
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_CompletedAppointment_ReturnsFailure()
    {
        Appointment appointment = CreateTestAppointmentWithStaff(OrgId);
        appointment.CheckIn(_fixedNow.AddMinutes(-15), Creator.Customer);
        appointment.Complete(_fixedNow.AddMinutes(-5));
        var command = new NoShowAppointmentCommand(appointment.Id);

        _userContextMock.Setup(c => c.OrganizationId).Returns(OrgId);
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.NoShowProhibited, result.Error);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
