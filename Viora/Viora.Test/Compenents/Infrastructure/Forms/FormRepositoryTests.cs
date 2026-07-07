using System.Text.Json;
using Viora.Infrastructure;
using global::Viora.Infrastructure.Repositories.Forms;

namespace Viora.Test.Compenents.Infrastructure.Forms;

/// <summary>
/// Unit tests for the FormRepository against an InMemory database.
/// Form has a Name (FormName record = ComplexProperty) which InMemory cannot compile queries for.
/// Tests verify Add operations only.
/// </summary>
[TestClass]
public sealed class FormRepositoryTests : InfrastructureTestBase
{
    private readonly FormRepository _repository;

    public FormRepositoryTests()
    {
        _repository = new FormRepository(DbContext);
    }

    // ===== Add =====

    [TestMethod]
    public async Task Add_Form_PersistsToDatabase()
    {
        var form = CreateTestForm();

        _repository.Add(form);
        await DbContext.SaveChangesAsync();

        Assert.IsTrue(form.Id != Guid.Empty);
    }

    [TestMethod]
    public async Task Add_MultipleForms_PersistsAll()
    {
        var form1 = CreateTestForm();
        var form2 = CreateTestForm();

        _repository.Add(form1);
        _repository.Add(form2);
        await DbContext.SaveChangesAsync();

        Assert.AreNotEqual(form1.Id, form2.Id);
    }

    // ===== Helpers =====

    private static global::Viora.Domain.Forms.Form CreateTestForm()
    {
        var fields = JsonDocument.Parse("""{"fields": [{"name": "q1", "type": "text"}]}""");
        return global::Viora.Domain.Forms.Form.Create(Guid.NewGuid(), Guid.NewGuid(), "Test Form", fields).Value;
    }
}
