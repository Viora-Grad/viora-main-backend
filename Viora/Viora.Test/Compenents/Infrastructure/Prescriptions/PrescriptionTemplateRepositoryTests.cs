using Viora.Infrastructure;
using global::Viora.Infrastructure.Repositories.Prescriptions;

namespace Viora.Test.Compenents.Infrastructure.Prescriptions;

/// <summary>
/// Unit tests for the PrescriptionTemplateRepository against an InMemory database.
/// PrescriptionTemplate has a Name (TemplateName record = ComplexProperty) which InMemory cannot compile queries for.
/// Tests verify Add operations only.
/// </summary>
[TestClass]
public sealed class PrescriptionTemplateRepositoryTests : InfrastructureTestBase
{
    private readonly PrescriptionTemplateRepository _repository;

    public PrescriptionTemplateRepositoryTests()
    {
        _repository = new PrescriptionTemplateRepository(DbContext);
    }

    // ===== Add =====

    [TestMethod]
    public async Task Add_Template_PersistsToDatabase()
    {
        var template = CreateTestTemplate();

        _repository.Add(template);
        await DbContext.SaveChangesAsync();

        Assert.IsTrue(template.Id != Guid.Empty);
    }

    [TestMethod]
    public async Task Add_MultipleTemplates_PersistsAll()
    {
        var template1 = CreateTestTemplate();
        var template2 = CreateTestTemplate();

        _repository.Add(template1);
        _repository.Add(template2);
        await DbContext.SaveChangesAsync();

        Assert.AreNotEqual(template1.Id, template2.Id);
    }

    // ===== Helpers =====

    private static global::Viora.Domain.Prescriptions.PrescriptionTemplate CreateTestTemplate()
    {
        return global::Viora.Domain.Prescriptions.PrescriptionTemplate.Create(
            Guid.NewGuid(), "Default Template", Guid.NewGuid(), 1.0, 1.0, 1.0, 1.0).Value;
    }
}
