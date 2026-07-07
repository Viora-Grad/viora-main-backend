using Moq;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Prescriptions.CreatePrescription;
using Viora.Application.Prescriptions.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Prescriptions;

namespace Viora.Test.Compenents.Application.Prescriptions;

/// <summary>
/// Unit tests for the CreatePrescriptionCommandHandler covering successful creation, appointment not found, and invalid item scenarios.
/// </summary>
[TestClass]
public sealed class CreatePrescriptionCommandHandlerTests
{
    private readonly Mock<IAppointmentsRepository> _appointmentRepoMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly Mock<IPrescriptionRepository> _prescriptionRepoMock = new();
    private readonly Mock<IPrescriptionItemRepository> _prescriptionItemRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CreatePrescriptionCommandHandler _handler;

    public CreatePrescriptionCommandHandlerTests()
    {
        _handler = new CreatePrescriptionCommandHandler(
            _appointmentRepoMock.Object,
            _dateTimeProviderMock.Object,
            _prescriptionRepoMock.Object,
            _prescriptionItemRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_AppointmentNotFound_ThrowsNotFoundException()
    {
        Guid appointmentId = Guid.NewGuid();
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new CreatePrescriptionCommand(appointmentId, new List<PrescriptionItemDTO>()), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_ValidPrescriptionItems_CreatesPrescription()
    {
        Guid appointmentId = Guid.NewGuid();
        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(DateTime.UtcNow);
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestAppointment(appointmentId));
        _unitOfWorkMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var items = new List<PrescriptionItemDTO>
        {
            new("Aspirin", "Take after meal", "100mg", 2, 30),
            new("Ibuprofen", null, "200mg", 3, 14)
        };

        var result = await _handler.Handle(
            new CreatePrescriptionCommand(appointmentId, items), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreNotEqual(Guid.Empty, result.Value);
        _prescriptionRepoMock.Verify(r => r.Add(It.IsAny<Prescription>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_EmptyItems_CreatesPrescriptionWithoutItems()
    {
        Guid appointmentId = Guid.NewGuid();
        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(DateTime.UtcNow);
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestAppointment(appointmentId));
        _unitOfWorkMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(
            new CreatePrescriptionCommand(appointmentId, new List<PrescriptionItemDTO>()), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        _prescriptionRepoMock.Verify(r => r.Add(It.IsAny<Prescription>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_ValidPrescription_SavesChanges()
    {
        Guid appointmentId = Guid.NewGuid();
        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(DateTime.UtcNow);
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestAppointment(appointmentId));
        _unitOfWorkMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var items = new List<PrescriptionItemDTO>
        {
            new("Aspirin", null, "100mg", 1, 7)
        };

        await _handler.Handle(
            new CreatePrescriptionCommand(appointmentId, items), CancellationToken.None);

        _unitOfWorkMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
}
