using Moq;
using System.Reflection;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Appointments.GetAppointment;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;
using Viora.Domain.Branches;
using Viora.Domain.Services;
using Viora.Domain.Services.Internals;
using Viora.Domain.Shared;
using Viora.Domain.Shared.Internal;
using Viora.Domain.Staffs;
using Viora.Domain.Staffs.Internal;

namespace Viora.Test.Compenents.Application.Appointments;

[TestClass]
public sealed class GetAppointmentQueryHandlerTests
{
    private readonly Mock<IAppointmentsRepository> _appointmentRepoMock = new();
    private readonly GetAppointmentQueryHandler _handler;

    public GetAppointmentQueryHandlerTests()
    {
        _handler = new GetAppointmentQueryHandler(_appointmentRepoMock.Object);
    }

    [TestMethod]
    public async Task Handle_AppointmentFound_ReturnsResponse()
    {
        var appointment = Appointment.Book(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null,
            new DateTime(2026, 7, 15, 10, 0, 0, DateTimeKind.Utc),
            1, PaymentMethod.Cash, null, Creator.Customer, Platform.Web, 30,
            new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc));

        var staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);
        typeof(Staff).GetProperty("FirstName")!.SetValue(staff, (FirstName)"John");
        typeof(Staff).GetProperty("LastName")!.SetValue(staff, (LastName)"Doe");
        typeof(Staff).GetProperty("PhoneNumber")!.SetValue(staff, (PhoneNumber)"+1234567890");
        typeof(Appointment).GetProperty("Staff",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(appointment, staff);

        var service = (Service)Activator.CreateInstance(typeof(Service), nonPublic: true)!;
        typeof(Service).GetProperty("Name")!.SetValue(service, (ServiceName)"Test Service");
        typeof(Service).GetProperty("Cost")!.SetValue(service, new Money(100m, Currency.Usd));
        typeof(Appointment).GetProperty("Service")!.SetValue(appointment, service);

        var branch = (Branch)Activator.CreateInstance(typeof(Branch), nonPublic: true)!;
        typeof(Branch).GetProperty("Address")!.SetValue(branch,
            new Address(1, "Main St", "City", "State", Guid.NewGuid(), 12345));
        typeof(Appointment).GetProperty("Branch")!.SetValue(appointment, branch);

        var query = new GetAppointmentQuery(appointment.Id);

        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        Result<GetAppointmentResponse> result = await _handler.Handle(query, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(appointment.Id, result.Value.AppointmentId);
    }

    [TestMethod]
    public async Task Handle_AppointmentNotFound_ThrowsNotFoundException()
    {
        var query = new GetAppointmentQuery(Guid.NewGuid());

        _appointmentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(query, CancellationToken.None));
    }
}
