using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetTopologySuite.Geometries;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Events;
using Viora.Domain.Appointments.Internal;
using Viora.Domain.Branches;
using Viora.Domain.Services;
using Viora.Domain.Shared;
using Viora.Domain.Shared.Internal;
using Viora.Domain.Staffs;
using Viora.Domain.Staffs.Internal;
using Viora.Domain.Users.Identity;
using Viora.Infrastructure;

namespace Viora.Test.Integerations;

[TestClass]
public sealed class AppointmentsIntegrationTests
{
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly ApplicationDbContext _dbContext;
    private static readonly Guid OrgId = Guid.NewGuid();
    private static readonly DateTime FixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

    private static readonly Mock<IServiceSettings> ServiceSettingsMock = new();

    public AppointmentsIntegrationTests()
    {
        ServiceSettingsMock.Setup(s => s.SlotSizeInMinutes).Returns(15);
        ServiceSettingsMock.Setup(s => s.MinimumDurationInMinutes).Returns(15);
        ServiceSettingsMock.Setup(s => s.MaximumDurationInMinutes).Returns(240);
        ServiceSettingsMock.Setup(s => s.MaxGallerySize).Returns(10);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options, _publisherMock.Object);
    }

    [TestCleanup]
    public void Cleanup() => _dbContext.Dispose();

    [TestMethod]
    public async Task ScheduleAppointment_EndToEnd_PersistsAndRaisesDomainEvent()
    {
        var address = new Address(1, "Clinic St", "Giza", "Giza", Guid.NewGuid(), 12345);
        var point = new Point(31.2357, 30.0444);
        var branchResult = Branch.Create(OrgId, address, point, "clinic@test.com",
            [ServiceType.Cardiology], FixedNow);
        Assert.IsTrue(branchResult.IsSuccess);
        var branch = branchResult.Value;

        var money = new Money(300m, Currency.Usd);
        var serviceResult = Service.Create(branch.Id, "Cardiology Checkup", "Full heart checkup",
            30, ServiceType.Cardiology, money, ServiceSettingsMock.Object);
        Assert.IsTrue(serviceResult.IsSuccess);
        var service = serviceResult.Value;

        var staff = Staff.Create(OrgId, FixedNow);
        staff.SetStaffProperties("Sara", "Mohamed", "sara_m", "hashed_pw",
            new DateOnly(1985, 5, 15), Gender.Female, "+201001234567");
        staff.AddRoles([new Role("Doctor", null, OrgId)]);
        staff.AssignBranches([branch]);
        staff.Activate();

        _dbContext.AddRange(branch, service, staff);
        await _dbContext.SaveChangesAsync();

        var reservationDate = FixedNow.AddDays(1);

        var appointment = Appointment.Book(
            customerId: null,
            serviceId: service.Id,
            staffId: staff.Id,
            branchId: branch.Id,
            paymentId: null,
            reservationDate: reservationDate,
            appointmentQueueNumber: 1,
            payMethod: PaymentMethod.Cash,
            status: null,
            createdBy: Creator.Staff,
            requestPlatform: Platform.Web,
            estimatedDurationMinutes: 30,
            createdAt: FixedNow);

        Assert.AreEqual(1, appointment.DomainEvents.Count);
        Assert.IsInstanceOfType<AppointmentBookedEvent>(appointment.DomainEvents.First());
        var bookedEvent = (AppointmentBookedEvent)appointment.DomainEvents.First();
        Assert.AreEqual(appointment.Id, bookedEvent.AppointmentId);

        _dbContext.Add(appointment);
        await _dbContext.SaveChangesAsync();

        var retrieved = await _dbContext.Set<Appointment>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == appointment.Id);

        Assert.IsNotNull(retrieved);
        Assert.AreEqual(service.Id, retrieved.ServiceId);
        Assert.AreEqual(staff.Id, retrieved.StaffId);
        Assert.AreEqual(branch.Id, retrieved.BranchId);
        Assert.AreEqual(CustomerStatus.NotArrived, retrieved.Status);
        Assert.AreEqual(PaymentMethod.Cash, retrieved.PayMethod);
        Assert.AreEqual(Creator.Staff, retrieved.CreatedBy);
        Assert.AreEqual(Platform.Web, retrieved.RequestPlatform);
        Assert.IsFalse(retrieved.IsCheckedIn);
        Assert.AreEqual(1, retrieved.AppointmentQueueNumber);
        Assert.AreEqual(reservationDate, retrieved.ReservationDate);
    }
}
