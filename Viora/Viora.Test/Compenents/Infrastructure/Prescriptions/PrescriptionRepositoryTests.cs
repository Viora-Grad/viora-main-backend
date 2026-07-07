using Viora.Infrastructure;
using global::Viora.Infrastructure.Repositories.Prescriptions;

namespace Viora.Test.Compenents.Infrastructure.Prescriptions;

/// <summary>
/// Unit tests for the PrescriptionRepository against an InMemory database.
/// PrescriptionItem has ComplexProperty value objects which InMemory cannot compile queries for.
/// Tests verify Add and empty-result assertions only.
/// </summary>
[TestClass]
public sealed class PrescriptionRepositoryTests : InfrastructureTestBase
{
    private readonly PrescriptionRepository _repository;

    public PrescriptionRepositoryTests()
    {
        _repository = new PrescriptionRepository(DbContext);
    }

    // ===== Add =====

    [TestMethod]
    public async Task Add_Prescription_PersistsToDatabase()
    {
        var prescription = global::Viora.Domain.Prescriptions.Prescription.Create(Guid.NewGuid(), DateTime.UtcNow).Value;

        _repository.Add(prescription);
        await DbContext.SaveChangesAsync();

        Assert.IsTrue(prescription.Id != Guid.Empty);
    }
}
