using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.RealTimeScheduling.GetStaffShiftByDay;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.RealTimeScheduling;
using Viora.Domain.Staffs;

namespace Viora.Test.Compenents.Application.RealTimeScheduling;

/// <summary>
/// Unit tests for the GetStaffShiftByDayQueryHandler covering successful retrieval, not-found, and appointment slot mapping.
/// </summary>
[TestClass]
public sealed class GetStaffShiftByDayQueryHandlerTests
{
    private readonly Mock<IAppointmentsRepository> _appointmentRepoMock = new();
    private readonly Mock<IStaffRepository> _staffRepoMock = new();
    private readonly Mock<IShiftRepository> _shiftRepoMock = new();
    private readonly GetStaffShiftByDayQueryHandler _handler;

    public GetStaffShiftByDayQueryHandlerTests()
    {
        _handler = new GetStaffShiftByDayQueryHandler(
            _appointmentRepoMock.Object,
            _staffRepoMock.Object,
            _shiftRepoMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_StaffNotFound_ThrowsNotFoundException()
    {
        _staffRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Staff?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetStaffShiftByDayQuery(DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_ShiftNotFound_ThrowsNotFoundException()
    {
        var staff = CreateTestStaff();
        _staffRepoMock.Setup(r => r.GetByIdAsync(staff.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _shiftRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Shift?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetStaffShiftByDayQuery(DateTime.UtcNow, staff.Id, Guid.NewGuid()), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_ShiftExists_ReturnsStaffDayShiftResponse()
    {
        var staff = CreateTestStaff();
        var shift = Shift.Create(Guid.NewGuid(), new TimeOnly(9, 0), new TimeOnly(17, 0), staff.Id);

        _staffRepoMock.Setup(r => r.GetByIdAsync(staff.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _shiftRepoMock.Setup(r => r.GetByIdAsync(shift.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shift);
        _appointmentRepoMock.Setup(r => r.ListAsync(It.IsAny<ISpecification<Appointment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>());

        var result = await _handler.Handle(
            new GetStaffShiftByDayQuery(DateTime.UtcNow, staff.Id, shift.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(shift.Id, result.Value.ShiftId);
        Assert.AreEqual(staff.Id, result.Value.StaffId);
        Assert.AreEqual(0, result.Value.TimeReserved.Count);
    }

    [TestMethod]
    public async Task Handle_ShiftWithAppointments_ReturnsSlots()
    {
        var staff = CreateTestStaff();
        var shift = Shift.Create(Guid.NewGuid(), new TimeOnly(9, 0), new TimeOnly(17, 0), staff.Id);
        var appointment = CreateTestAppointment();

        _staffRepoMock.Setup(r => r.GetByIdAsync(staff.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);
        _shiftRepoMock.Setup(r => r.GetByIdAsync(shift.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shift);
        _appointmentRepoMock.Setup(r => r.ListAsync(It.IsAny<ISpecification<Appointment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment> { appointment });

        var result = await _handler.Handle(
            new GetStaffShiftByDayQuery(DateTime.UtcNow, staff.Id, shift.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Value.TimeReserved.Count);
    }

    // ===== Helpers =====

    private static Staff CreateTestStaff()
    {
        return Staff.Create(Guid.NewGuid(), DateTime.UtcNow);
    }

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
