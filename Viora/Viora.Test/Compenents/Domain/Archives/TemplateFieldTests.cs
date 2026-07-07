using Viora.Domain.Archives;
using Viora.Domain.Archives.Internals;

namespace Viora.Test.Compenents.Domain.Archives;

/// <summary>
/// Unit tests for the TemplateField entity covering the Create factory method.
/// </summary>
[TestClass]
public sealed class TemplateFieldTests
{
    // ===== Create =====

    /// <summary>
    /// Verifies that Create with valid input returns a TemplateField with all properties correctly set.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsTemplateFieldWithCorrectProperties()
    {
        // Arrange
        Guid templateVersionId = Guid.NewGuid();
        TemplateName name = new("PatientName");
        TemplateFieldLabel label = new("Patient Name");
        FieldType fieldType = FieldType.Text;
        bool required = true;
        int order = 0;
        FieldValidation validation = new(true, 1, 200, null, null, null);
        FieldLayout layout = new(1, 0, null, 12);

        // Act
        TemplateField field = TemplateField.Create(templateVersionId, name, label, fieldType, required, order, validation, layout);

        // Assert
        Assert.IsNotNull(field);
        Assert.AreNotEqual(Guid.Empty, field.Id);
        Assert.AreEqual(templateVersionId, field.TemplateVersionId);
        Assert.AreEqual(name, field.Name);
        Assert.AreEqual(label, field.Label);
        Assert.AreEqual(fieldType, field.Type);
        Assert.AreEqual(required, field.Required);
        Assert.AreEqual(order, field.Order);
        Assert.AreEqual(validation, field.Validation);
        Assert.AreEqual(layout, field.Layout);
    }

    /// <summary>
    /// Verifies that Create generates a new unique Id for each template field.
    /// </summary>
    [TestMethod]
    public void Create_CalledTwice_GeneratesDifferentIds()
    {
        // Arrange
        Guid templateVersionId = Guid.NewGuid();
        FieldValidation validation = new(false, null, null, null, null, null);
        FieldLayout layout = new(1, 0, null, 6);

        // Act
        TemplateField f1 = TemplateField.Create(templateVersionId, new("A"), new("Label A"), FieldType.Text, false, 0, validation, layout);
        TemplateField f2 = TemplateField.Create(templateVersionId, new("B"), new("Label B"), FieldType.Number, false, 1, validation, layout);

        // Assert
        Assert.AreNotEqual(f1.Id, f2.Id);
    }

    /// <summary>
    /// Verifies that Create with a specific template version ID links it correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithSpecificTemplateVersionId_SetsTemplateVersionIdCorrectly()
    {
        // Arrange
        Guid templateVersionId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        FieldValidation validation = new(false, null, null, null, null, null);
        FieldLayout layout = new(1, 0, null, 12);

        // Act
        TemplateField field = TemplateField.Create(templateVersionId, new("F"), new("L"), FieldType.Text, false, 0, validation, layout);

        // Assert
        Assert.AreEqual(templateVersionId, field.TemplateVersionId);
    }

    /// <summary>
    /// Verifies that Create with required true stores Required correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithRequiredTrue_StoresRequiredCorrectly()
    {
        // Arrange
        FieldValidation validation = new(true, null, null, null, null, null);
        FieldLayout layout = new(1, 0, null, 12);

        // Act
        TemplateField field = TemplateField.Create(Guid.NewGuid(), new("F"), new("L"), FieldType.Text, true, 0, validation, layout);

        // Assert
        Assert.IsTrue(field.Required);
    }

    /// <summary>
    /// Verifies that Create with required false stores Required correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithRequiredFalse_StoresRequiredCorrectly()
    {
        // Arrange
        FieldValidation validation = new(false, null, null, null, null, null);
        FieldLayout layout = new(1, 0, null, 12);

        // Act
        TemplateField field = TemplateField.Create(Guid.NewGuid(), new("F"), new("L"), FieldType.Text, false, 0, validation, layout);

        // Assert
        Assert.IsFalse(field.Required);
    }

    /// <summary>
    /// Verifies that Create stores the order correctly.
    /// </summary>
    [TestMethod]
    public void Create_StoresOrderCorrectly()
    {
        // Arrange
        FieldValidation validation = new(false, null, null, null, null, null);
        FieldLayout layout = new(1, 0, null, 12);

        // Act
        TemplateField field = TemplateField.Create(Guid.NewGuid(), new("F"), new("L"), FieldType.Text, false, 5, validation, layout);

        // Assert
        Assert.AreEqual(5, field.Order);
    }

    /// <summary>
    /// Verifies that Create with different field types stores them correctly.
    /// </summary>
    [TestMethod]
    [DataRow(FieldType.Text)]
    [DataRow(FieldType.Number)]
    [DataRow(FieldType.Date)]
    [DataRow(FieldType.Boolean)]
    [DataRow(FieldType.File)]
    [DataRow(FieldType.Image)]
    public void Create_WithDifferentFieldTypes_StoresTypeCorrectly(FieldType fieldType)
    {
        // Arrange
        FieldValidation validation = new(false, null, null, null, null, null);
        FieldLayout layout = new(1, 0, null, 12);

        // Act
        TemplateField field = TemplateField.Create(Guid.NewGuid(), new("F"), new("L"), fieldType, false, 0, validation, layout);

        // Assert
        Assert.AreEqual(fieldType, field.Type);
    }

    /// <summary>
    /// Verifies that Create stores the validation correctly.
    /// </summary>
    [TestMethod]
    public void Create_StoresValidationCorrectly()
    {
        // Arrange
        FieldValidation validation = new(true, 5, 100, 0, 1000, @"^[a-z]+$");
        FieldLayout layout = new(1, 0, null, 12);

        // Act
        TemplateField field = TemplateField.Create(Guid.NewGuid(), new("F"), new("L"), FieldType.Text, false, 0, validation, layout);

        // Assert
        Assert.IsTrue(field.Validation.Required);
        Assert.AreEqual(5, field.Validation.MinLength);
        Assert.AreEqual(100, field.Validation.MaxLength);
        Assert.AreEqual(0, field.Validation.Min);
        Assert.AreEqual(1000, field.Validation.Max);
        Assert.AreEqual("^[a-z]+$", field.Validation.Regex);
    }

    /// <summary>
    /// Verifies that Create stores the layout correctly.
    /// </summary>
    [TestMethod]
    public void Create_StoresLayoutCorrectly()
    {
        // Arrange
        FieldValidation validation = new(false, null, null, null, null, null);
        FieldLayout layout = new(2, 3, "Personal", 8);

        // Act
        TemplateField field = TemplateField.Create(Guid.NewGuid(), new("F"), new("L"), FieldType.Text, false, 0, validation, layout);

        // Assert
        Assert.AreEqual(2, field.Layout.Column);
        Assert.AreEqual(3, field.Layout.Order);
        Assert.AreEqual("Personal", field.Layout.Tab);
        Assert.AreEqual(8, field.Layout.Width);
    }
}
