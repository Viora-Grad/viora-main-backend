using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Prescriptions.GetPrescriptionByAppointment;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Prescriptions;

namespace Viora.Test.Compenents.Application.Prescriptions;

/// <summary>
/// Unit tests for the GetPrescriptionByAppointmentQueryHandler covering successful retrieval and not-found scenarios.
/// </summary>
[TestClass]
public sealed class GetPrescriptionByAppointmentQueryHandlerTests
{
    private readonly Mock<IAppointmentsRepository> _appointmentRepoMock = new();
    private readonly Mock<IPrescriptionRepository> _prescriptionRepoMock = new();
    private readonly GetPrescriptionByAppointmentQueryHandler _handler;

    public GetPrescriptionByAppointmentQueryHandlerTests()
    {
        _handler = new GetPrescriptionByAppointmentQueryHandler(
            _appointmentRepoMock.Object,
            _prescriptionRepoMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_AppointmentNotFound_ThrowsNotFoundException()
    {
        Guid appointmentId = Guid.NewGuid();
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetPrescriptionByAppointmentQuery(appointmentId), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_AppointmentHasNoPrescription_ThrowsNotFoundException()
    {
        Guid appointmentId = Guid.NewGuid();
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestAppointment(appointmentId));
        _prescriptionRepoMock.Setup(r => r.GetByAppointmentIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Prescription?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetPrescriptionByAppointmentQuery(appointmentId), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_PrescriptionExists_ReturnsResponse()
    {
        Guid appointmentId = Guid.NewGuid();
        var prescription = CreateTestPrescription(appointmentId);

        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestAppointment(appointmentId));
        _prescriptionRepoMock.Setup(r => r.GetByAppointmentIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(prescription);

        var result = await _handler.Handle(
            new GetPrescriptionByAppointmentQuery(appointmentId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(prescription.Id, result.Value.Id);
        Assert.AreEqual(appointmentId, result.Value.AppointmentId);
    }

    [TestMethod]
    public async Task Handle_PrescriptionWithItems_ReturnsItems()
    {
        Guid appointmentId = Guid.NewGuid();
        var prescription = CreateTestPrescription(appointmentId);
        var item = Viora.Domain.Prescriptions.PrescriptionItem.Create(
            prescription.Id, "Ibuprofen", null, "200mg", 3, 14).Value;
        prescription.AddItems(new List<Viora.Domain.Prescriptions.PrescriptionItem> { item });

        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestAppointment(appointmentId));
        _prescriptionRepoMock.Setup(r => r.GetByAppointmentIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(prescription);

        var result = await _handler.Handle(
            new GetPrescriptionByAppointmentQuery(appointmentId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Value.Items.Count);
        Assert.AreEqual("Ibuprofen", result.Value.Items[0].Name);
    }

    // ===== Helpers =====

    private static Appointment CreateTestAppointment(Guid id)
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

    private static Prescription CreateTestPrescription(Guid appointmentId)
    {
        return Prescription.Create(appointmentId, DateTime.UtcNow).Value;
    }
}
