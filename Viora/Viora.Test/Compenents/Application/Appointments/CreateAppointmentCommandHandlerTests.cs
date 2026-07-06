using MediatR;
using Moq;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Appointments.CreateAppointment;
using Viora.Application.Appointments.Shared;
using Viora.Application.Wallets.PromisePayment;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.RealTimeScheduling;
using Viora.Domain.Services;
using Viora.Domain.Shared;
using Viora.Domain.Shared.Internal;
using Viora.Domain.Users.Customers;
using Viora.Domain.Users.Internal;

namespace Viora.Test.Compenents.Application.Appointments;

[TestClass]
public sealed class CreateAppointmentCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _customerRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IAppointmentsRepository> _appointmentRepoMock = new();
    private readonly Mock<IScheduleRepository> _scheduleRepoMock = new();
    private readonly Mock<IShiftRepository> _shiftRepoMock = new();
    private readonly Mock<IServiceRepository> _serviceRepoMock = new();
    private readonly Mock<ISender> _senderMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly Mock<IServiceSettings> _serviceSettingsMock = new();
    private readonly CreateAppointmentCommandHandler _handler;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OrgId = Guid.NewGuid();
    private static readonly Guid ServiceId = Guid.NewGuid();
    private static readonly Guid StaffId = Guid.NewGuid();
    private static readonly Guid BranchId = Guid.NewGuid();
    private readonly DateTime _reservationDate = new(2026, 7, 15, 10, 0, 0, DateTimeKind.Utc);
    private readonly DateTime _fixedNow = new(2026, 7, 14, 0, 0, 0, DateTimeKind.Utc);

    public CreateAppointmentCommandHandlerTests()
    {
        _clockMock.Setup(c => c.UtcNow).Returns(_fixedNow);
        _handler = new CreateAppointmentCommandHandler(
            _customerRepoMock.Object,
            _unitOfWorkMock.Object,
            _appointmentRepoMock.Object,
            _scheduleRepoMock.Object,
            _shiftRepoMock.Object,
            _serviceRepoMock.Object,
            _senderMock.Object,
            _userContextMock.Object,
            _clockMock.Object);
    }

    private Service CreateTestService(Guid branchId)
    {
        _serviceSettingsMock.Setup(s => s.SlotSizeInMinutes).Returns(15);
        _serviceSettingsMock.Setup(s => s.MinimumDurationInMinutes).Returns(15);
        _serviceSettingsMock.Setup(s => s.MaximumDurationInMinutes).Returns(240);
        _serviceSettingsMock.Setup(s => s.MaxGallerySize).Returns(10);

        var money = new Money(200m, Currency.Usd);
        var result = Service.Create(branchId, "Haircut", "A haircut service",
            30, ServiceType.Cardiology, money, _serviceSettingsMock.Object);
        return result.Value;
    }

    private void SetupCommonSuccessMocks()
    {
        var service = CreateTestService(BranchId);
        var schedule = Schedule.Create(BranchId, DayOfWeek.Wednesday);
        var shift = Shift.Create(schedule.Id,
            new TimeOnly(9, 0), new TimeOnly(17, 0), StaffId);

        _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _scheduleRepoMock.Setup(r => r.getByBranchIdAndDayAsync(
            BranchId, DayOfWeek.Wednesday, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        _shiftRepoMock.Setup(r => r.GetActiveShiftAsync(
            schedule.Id, StaffId, It.IsAny<TimeOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(shift);
        _appointmentRepoMock.Setup(r => r.OverlapsAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _appointmentRepoMock.Setup(r => r.CountAsync(
            It.IsAny<GetAppointmentsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
    }

    [TestMethod]
    public async Task Handle_CustomerWithCash_ReturnsAppointmentId()
    {
        var customer = Customer.Create(
            Guid.NewGuid(),
            null,
            new PersonalInfo("John", "Doe", new DateOnly(1990, 1, 1), Gender.Male),
            _fixedNow, ["+1234567890"], [new Viora.Domain.Users.Internal.Email("john@example.com")]);

        _userContextMock.Setup(c => c.UserId).Returns(UserId);
        _userContextMock.Setup(c => c.OrganizationId).Returns((Guid?)null);
        _customerRepoMock.Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        SetupCommonSuccessMocks();

        var command = new CreateAppointmentCommand(
            ServiceId, StaffId, null, _reservationDate,
            "Cash", null, "Customer", "Web");

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreNotEqual(Guid.Empty, result.Value);
        _appointmentRepoMock.Verify(r => r.Add(It.IsAny<Appointment>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_ServiceNotFound_ThrowsNotFoundException()
    {
        _userContextMock.Setup(c => c.UserId).Returns(UserId);
        _userContextMock.Setup(c => c.OrganizationId).Returns(OrgId);
        _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        var command = new CreateAppointmentCommand(
            ServiceId, StaffId, null, _reservationDate,
            "Cash", null, "Customer", "Web");

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_OverlappingAppointment_ReturnsFailure()
    {
        _userContextMock.Setup(c => c.UserId).Returns(UserId);
        _userContextMock.Setup(c => c.OrganizationId).Returns(OrgId);

        var service = CreateTestService(BranchId);
        var schedule = Schedule.Create(BranchId, DayOfWeek.Wednesday);
        var shift = Shift.Create(schedule.Id,
            new TimeOnly(9, 0), new TimeOnly(17, 0), StaffId);

        _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _scheduleRepoMock.Setup(r => r.getByBranchIdAndDayAsync(
            BranchId, DayOfWeek.Wednesday, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        _shiftRepoMock.Setup(r => r.GetActiveShiftAsync(
            schedule.Id, StaffId, It.IsAny<TimeOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(shift);
        _appointmentRepoMock.Setup(r => r.OverlapsAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateAppointmentCommand(
            ServiceId, StaffId, null, _reservationDate,
            "Cash", null, "Staff", "Web");

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.AppointmentTimeConflict, result.Error);
    }

    [TestMethod]
    public async Task Handle_WalletPayment_ProcessesPromise()
    {
        var customer = Customer.Create(
            Guid.NewGuid(),
            null,
            new PersonalInfo("John", "Doe", new DateOnly(1990, 1, 1), Gender.Male),
            _fixedNow, [new PhoneNumber("+1234567890")], [new Viora.Domain.Users.Internal.Email("john@example.com")]);
        var promisePaymentId = Guid.NewGuid();

        _userContextMock.Setup(c => c.UserId).Returns(UserId);
        _userContextMock.Setup(c => c.OrganizationId).Returns((Guid?)null);
        _customerRepoMock.Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        SetupCommonSuccessMocks();

        _senderMock.Setup(s => s.Send(
            It.IsAny<PromisePaymentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(promisePaymentId));

        var command = new CreateAppointmentCommand(
            ServiceId, StaffId, null, _reservationDate,
            "Wallet", null, "Customer", "Web");

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        _senderMock.Verify(s => s.Send(
            It.IsAny<PromisePaymentCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
