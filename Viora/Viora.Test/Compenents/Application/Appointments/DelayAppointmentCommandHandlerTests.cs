using Moq;
using System.Reflection;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Appointments.DelayAppointment;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;
using Viora.Domain.Staffs;

namespace Viora.Test.Compenents.Application.Appointments;

[TestClass]
public sealed class DelayAppointmentCommandHandlerTests
{
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IAppointmentsRepository> _appointmentRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly DelayAppointmentCommandHandler _handler;

    private static readonly Guid BranchId = Guid.NewGuid();
    private static readonly Guid OrgId = Guid.NewGuid();

    public DelayAppointmentCommandHandlerTests()
    {
        _handler = new DelayAppointmentCommandHandler(
            _userContextMock.Object,
            _appointmentRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    private Appointment CreateBookedAppointment(DateTime? reservationDate = null, int duration = 30)
    {
        return Appointment.Book(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), BranchId, null,
            reservationDate ?? new DateTime(2026, 7, 6, 10, 0, 0, DateTimeKind.Utc),
            1, PaymentMethod.Cash, null,
            Creator.Staff, Platform.Web, duration, DateTime.UtcNow.AddDays(-1));
    }

    /// <summary>
    /// Creates an Appointment with the Staff navigation property set via reflection.
    /// The handler accesses appointment.Staff.OrganizationId for org authorization.
    /// </summary>
    private Appointment CreateBookedAppointmentWithStaff(Guid orgId, DateTime? reservationDate = null, int duration = 30)
    {
        var appointment = CreateBookedAppointment(reservationDate, duration);
        var staff = Staff.Create(orgId, DateTime.UtcNow);
        var prop = typeof(Appointment).GetProperty("Staff",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        prop!.SetValue(appointment, staff);
        return appointment;
    }

    [TestMethod]
    public async Task Handle_ValidDelay_ReturnsSuccess()
    {
        var appointment = CreateBookedAppointmentWithStaff(OrgId);
        var command = new DelayAppointmentCommand(appointment.Id, TimeSpan.FromMinutes(15));

        _userContextMock.Setup(c => c.OrganizationId).Returns(OrgId);
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _appointmentRepoMock.Setup(r => r.GetByDateRangeAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<IEnumerable<Appointment>>(new List<Appointment>()));

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_AppointmentNotFound_ReturnsFailure()
    {
        var command = new DelayAppointmentCommand(Guid.NewGuid(), TimeSpan.FromMinutes(15));

        _userContextMock.Setup(c => c.OrganizationId).Returns(OrgId);
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        Result result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.AppointmentNotFound, result.Error);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_DifferentOrg_ThrowsUnauthorizedAccessException()
    {
        var appointment = CreateBookedAppointmentWithStaff(OrgId);
        var command = new DelayAppointmentCommand(appointment.Id, TimeSpan.FromMinutes(15));

        _userContextMock.Setup(c => c.OrganizationId).Returns(Guid.NewGuid());
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

}
