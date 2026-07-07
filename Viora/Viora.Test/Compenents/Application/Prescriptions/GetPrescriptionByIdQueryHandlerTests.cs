using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Prescriptions.GetPrescriptionById;
using Viora.Domain.Abstractions;
using Viora.Domain.Prescriptions;

namespace Viora.Test.Compenents.Application.Prescriptions;

/// <summary>
/// Unit tests for the GetPrescriptionByIdQueryHandler covering successful retrieval and not-found scenarios.
/// </summary>
[TestClass]
public sealed class GetPrescriptionByIdQueryHandlerTests
{
    private readonly Mock<IPrescriptionRepository> _prescriptionRepoMock = new();
    private readonly GetPrescriptionByIdQueryHandler _handler;

    public GetPrescriptionByIdQueryHandlerTests()
    {
        _handler = new GetPrescriptionByIdQueryHandler(_prescriptionRepoMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_PrescriptionNotFound_ThrowsNotFoundException()
    {
        Guid prescriptionId = Guid.NewGuid();
        _prescriptionRepoMock.Setup(r => r.GetByIdAsync(prescriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Prescription?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetPrescriptionByIdQuery(prescriptionId), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_PrescriptionExists_ReturnsResponse()
    {
        var prescription = CreateTestPrescription();

        _prescriptionRepoMock.Setup(r => r.GetByIdAsync(prescription.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(prescription);

        var result = await _handler.Handle(
            new GetPrescriptionByIdQuery(prescription.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(prescription.Id, result.Value.Id);
        Assert.AreEqual(prescription.AppointmentId, result.Value.AppointmentId);
    }

    [TestMethod]
    public async Task Handle_PrescriptionWithItems_ReturnsItems()
    {
        var prescription = CreateTestPrescription();
        var item = Viora.Domain.Prescriptions.PrescriptionItem.Create(
            prescription.Id, "Aspirin", "Take after meal", "100mg", 2, 30).Value;
        prescription.AddItems(new List<Viora.Domain.Prescriptions.PrescriptionItem> { item });

        _prescriptionRepoMock.Setup(r => r.GetByIdAsync(prescription.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(prescription);

        var result = await _handler.Handle(
            new GetPrescriptionByIdQuery(prescription.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Value.Items.Count);
        Assert.AreEqual("Aspirin", result.Value.Items[0].Name);
        Assert.AreEqual("100mg", result.Value.Items[0].Dose);
    }

    // ===== Helpers =====

    private static Prescription CreateTestPrescription()
    {
        return Prescription.Create(Guid.NewGuid(), DateTime.UtcNow).Value;
    }
}
