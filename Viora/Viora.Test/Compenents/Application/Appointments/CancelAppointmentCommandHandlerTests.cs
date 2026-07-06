using Moq;
using System.Reflection;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Appointments.CancelAppointment;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;
using Viora.Domain.Staffs;
using Viora.Domain.Users.Customers;
using Viora.Domain.Users.Internal;

namespace Viora.Test.Compenents.Application.Appointments;

[TestClass]
public sealed class CancelAppointmentCommandHandlerTests
{
    private static readonly Guid BranchId = Guid.NewGuid();
    private static readonly Guid StaffId = Guid.NewGuid();
    private static readonly Guid CustomerId = Guid.NewGuid();
    private static readonly Guid OrgId = Guid.NewGuid();

    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<ICustomerRepository> _customerRepoMock = new();
    private readonly Mock<IStaffRepository> _staffRepoMock = new();
    private readonly Mock<IAppointmentsRepository> _appointmentRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();

    private readonly DateTime _fixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);
    private readonly DateTime _reservationDate = new(2026, 7, 15, 10, 0, 0, DateTimeKind.Utc);
    private readonly CancelAppointmentCommandHandler _handler;

    private static readonly PersonalInfo PersonalInfo = new(
        "John", "Doe", new DateOnly(1990, 1, 1), Gender.Male);

    public CancelAppointmentCommandHandlerTests()
    {
        _clockMock.Setup(c => c.UtcNow).Returns(_fixedNow);

        _handler = new CancelAppointmentCommandHandler(
            _userContextMock.Object,
            _customerRepoMock.Object,
            _staffRepoMock.Object,
            _appointmentRepoMock.Object,
            _unitOfWorkMock.Object,
            _clockMock.Object);
    }

    private Appointment CreateTestAppointment(
        CustomerStatus status = CustomerStatus.NotArrived,
        DateTime? reservationDate = null)
    {
        var resDate = reservationDate ?? _reservationDate;
        return Appointment.Book(
            CustomerId, Guid.NewGuid(), StaffId, BranchId, null,
            resDate, 1, PaymentMethod.Cash, status == CustomerStatus.NotArrived ? null : status,
            Creator.Customer, Platform.Web, 30, _fixedNow.AddDays(-14));
    }

    /// <summary>
    /// Creates an Appointment with the Staff navigation property set via reflection.
    /// The handler accesses appointment.Staff.OrganizationId in the staff cancellation path.
    /// </summary>
    private Appointment CreateTestAppointmentWithStaff(
        Guid orgId, CustomerStatus status = CustomerStatus.NotArrived)
    {
        var appointment = CreateTestAppointment(status);
        var staff = Staff.Create(orgId, _fixedNow, StaffId);
        var prop = typeof(Appointment).GetProperty("Staff",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        prop!.SetValue(appointment, staff);
        return appointment;
    }

    // ===== Customer Path =====

    [TestMethod]
    public async Task Handle_CustomerCancelsOutsideWindow_ReturnsSuccess()
    {
        Appointment appointment = CreateTestAppointment();
        var command = new CancelAppointmentCommand(appointment.Id);

        _userContextMock.Setup(c => c.UserId).Returns(CustomerId);
        _customerRepoMock
            .Setup(r => r.GetByIdAsync(CustomerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Customer.Create(CustomerId, null, PersonalInfo, _fixedNow, null, null));
        _appointmentRepoMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(CustomerStatus.Canceled, appointment.Status);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_CustomerCancelsInsideTwoHourWindow_ReturnsFailure()
    {
        var soonReservation = _fixedNow.AddHours(1);
        Appointment appointment = CreateTestAppointment(reservationDate: soonReservation);
        var command = new CancelAppointmentCommand(appointment.Id);

        _userContextMock.Setup(c => c.UserId).Returns(CustomerId);
        _customerRepoMock
            .Setup(r => r.GetByIdAsync(CustomerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Customer.Create(CustomerId, null, PersonalInfo, _fixedNow, null, null));
        _appointmentRepoMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.CancellationProhibited, result.Error);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ===== Staff Path =====

    [TestMethod]
    public async Task Handle_StaffCancelsSameOrg_ReturnsSuccess()
    {
        Appointment appointment = CreateTestAppointmentWithStaff(OrgId);
        var command = new CancelAppointmentCommand(appointment.Id);

        _userContextMock.Setup(c => c.UserId).Returns(Guid.NewGuid());
        _customerRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);
        _userContextMock.Setup(c => c.OrganizationId).Returns(OrgId);
        _appointmentRepoMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(CustomerStatus.Canceled, appointment.Status);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_StaffDifferentOrg_ThrowsUnauthorizedAccessException()
    {
        Appointment appointment = CreateTestAppointmentWithStaff(OrgId);
        var command = new CancelAppointmentCommand(appointment.Id);

        _userContextMock.Setup(c => c.UserId).Returns(Guid.NewGuid());
        _customerRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);
        _userContextMock.Setup(c => c.OrganizationId).Returns(Guid.NewGuid());
        _appointmentRepoMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    // ===== Error Path =====

    [TestMethod]
    public async Task Handle_AppointmentNotFound_ThrowsNotFoundException()
    {
        var appointmentId = Guid.NewGuid();
        var command = new CancelAppointmentCommand(appointmentId);

        _appointmentRepoMock
            .Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
