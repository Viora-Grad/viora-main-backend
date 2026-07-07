using Viora.Infrastructure;
using global::Viora.Infrastructure.Repositories.Prescriptions;

namespace Viora.Test.Compenents.Infrastructure.Prescriptions;

/// <summary>
/// Unit tests for the PrescriptionItemRepository against an InMemory database.
/// PrescriptionItem has ComplexProperty value objects (MedicationName, MedicalDosage, PrescriptionNote).
/// Tests verify Add operations only.
/// </summary>
[TestClass]
public sealed class PrescriptionItemRepositoryTests : InfrastructureTestBase
{
    private readonly PrescriptionItemRepository _repository;

    public PrescriptionItemRepositoryTests()
    {
        _repository = new PrescriptionItemRepository(DbContext);
    }

    // ===== AddRange =====

    [TestMethod]
    public async Task AddRange_MultipleItems_PersistsAll()
    {
        var prescriptionId = Guid.NewGuid();
        var items = new[]
        {
            global::Viora.Domain.Prescriptions.PrescriptionItem.Create(prescriptionId, "Aspirin", "Take after meal", "500mg", 2, 7).Value,
            global::Viora.Domain.Prescriptions.PrescriptionItem.Create(prescriptionId, "Ibuprofen", "Take with food", "200mg", 3, 5).Value,
            global::Viora.Domain.Prescriptions.PrescriptionItem.Create(prescriptionId, "Paracetamol", null, "250mg", 1, 10).Value,
        };

        _repository.AddRange(items);
        await DbContext.SaveChangesAsync();

        foreach (var item in items)
        {
            Assert.IsTrue(item.Id != Guid.Empty);
        }
    }

    // ===== Add single item =====

    [TestMethod]
    public async Task Add_SingleItem_PersistsToDatabase()
    {
        var item = global::Viora.Domain.Prescriptions.PrescriptionItem.Create(
            Guid.NewGuid(), "Amoxicillin", "Complete full course", "500mg", 3, 14).Value;

        DbContext.Set<global::Viora.Domain.Prescriptions.PrescriptionItem>().Add(item);
        await DbContext.SaveChangesAsync();

        Assert.IsTrue(item.Id != Guid.Empty);
    }
}
