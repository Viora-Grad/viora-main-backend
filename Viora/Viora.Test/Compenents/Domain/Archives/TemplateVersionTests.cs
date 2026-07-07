using Viora.Domain.Archives;

namespace Viora.Test.Compenents.Domain.Archives;

/// <summary>
/// Unit tests for the TemplateVersion entity covering the Create factory method, Publish method, and AddField method.
/// </summary>
[TestClass]
public sealed class TemplateVersionTests
{
    // ===== Create =====

    /// <summary>
    /// Verifies that Create with valid input returns a TemplateVersion with all properties correctly set.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsTemplateVersionWithCorrectProperties()
    {
        // Arrange
        Guid templateId = Guid.NewGuid();
        int version = 1;
        DateTime createdAt = DateTime.UtcNow;

        // Act
        TemplateVersion templateVersion = TemplateVersion.Create(templateId, version, createdAt);

        // Assert
        Assert.IsNotNull(templateVersion);
        Assert.AreNotEqual(Guid.Empty, templateVersion.Id);
        Assert.AreEqual(templateId, templateVersion.TemplateId);
        Assert.AreEqual(version, templateVersion.Version);
        Assert.IsFalse(templateVersion.IsPublished);
        Assert.AreEqual(0, templateVersion.Fields.Count);
        Assert.AreEqual(createdAt, templateVersion.CreatedAt);
    }

    /// <summary>
    /// Verifies that Create generates a new unique Id for each template version.
    /// </summary>
    [TestMethod]
    public void Create_CalledTwice_GeneratesDifferentIds()
    {
        // Arrange
        Guid templateId = Guid.NewGuid();

        // Act
        TemplateVersion v1 = TemplateVersion.Create(templateId, 1, DateTime.UtcNow);
        TemplateVersion v2 = TemplateVersion.Create(templateId, 2, DateTime.UtcNow);

        // Assert
        Assert.AreNotEqual(v1.Id, v2.Id);
    }

    /// <summary>
    /// Verifies that Create with a specific template ID links it correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithSpecificTemplateId_SetsTemplateIdCorrectly()
    {
        // Arrange
        Guid templateId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        // Act
        TemplateVersion version = TemplateVersion.Create(templateId, 1, DateTime.UtcNow);

        // Assert
        Assert.AreEqual(templateId, version.TemplateId);
    }

    /// <summary>
    /// Verifies that Create initializes IsPublished to false.
    /// </summary>
    [TestMethod]
    public void Create_InitializesIsPublishedToFalse()
    {
        // Arrange & Act
        TemplateVersion version = TemplateVersion.Create(Guid.NewGuid(), 1, DateTime.UtcNow);

        // Assert
        Assert.IsFalse(version.IsPublished);
    }

    /// <summary>
    /// Verifies that Create initializes the Fields collection as empty.
    /// </summary>
    [TestMethod]
    public void Create_InitializesEmptyFieldsCollection()
    {
        // Arrange & Act
        TemplateVersion version = TemplateVersion.Create(Guid.NewGuid(), 1, DateTime.UtcNow);

        // Assert
        Assert.AreEqual(0, version.Fields.Count);
    }

    // ===== Publish =====

    /// <summary>
    /// Verifies that Publish sets IsPublished to true.
    /// </summary>
    [TestMethod]
    public void Publish_SetsIsPublishedToTrue()
    {
        // Arrange
        TemplateVersion version = TemplateVersion.Create(Guid.NewGuid(), 1, DateTime.UtcNow);

        // Act
        version.Publish();

        // Assert
        Assert.IsTrue(version.IsPublished);
    }

    /// <summary>
    /// Verifies that Publish can be called multiple times without error.
    /// </summary>
    [TestMethod]
    public void Publish_CalledMultipleTimes_RemainsPublished()
    {
        // Arrange
        TemplateVersion version = TemplateVersion.Create(Guid.NewGuid(), 1, DateTime.UtcNow);

        // Act
        version.Publish();
        version.Publish();

        // Assert
        Assert.IsTrue(version.IsPublished);
    }

    // ===== AddField =====

    /// <summary>
    /// Verifies that AddField adds a field to the Fields collection.
    /// </summary>
    [TestMethod]
    public void AddField_SingleField_AddsToCollection()
    {
        // Arrange
        TemplateVersion version = TemplateVersion.Create(Guid.NewGuid(), 1, DateTime.UtcNow);
        TemplateField field = CreateTestField(version.Id);

        // Act
        version.AddField(field);

        // Assert
        Assert.AreEqual(1, version.Fields.Count);
        Assert.IsTrue(version.Fields.Contains(field));
    }

    /// <summary>
    /// Verifies that AddField with multiple fields adds all to the collection.
    /// </summary>
    [TestMethod]
    public void AddField_MultipleFields_AddsAllToCollection()
    {
        // Arrange
        TemplateVersion version = TemplateVersion.Create(Guid.NewGuid(), 1, DateTime.UtcNow);
        TemplateField f1 = CreateTestField(version.Id);
        TemplateField f2 = CreateTestField(version.Id);
        TemplateField f3 = CreateTestField(version.Id);

        // Act
        version.AddField(f1);
        version.AddField(f2);
        version.AddField(f3);

        // Assert
        Assert.AreEqual(3, version.Fields.Count);
        Assert.IsTrue(version.Fields.Contains(f1));
        Assert.IsTrue(version.Fields.Contains(f2));
        Assert.IsTrue(version.Fields.Contains(f3));
    }

    /// <summary>
    /// Verifies that AddField does not remove previously added fields.
    /// </summary>
    [TestMethod]
    public void AddField_AfterPreviousField_KeepsPreviousField()
    {
        // Arrange
        TemplateVersion version = TemplateVersion.Create(Guid.NewGuid(), 1, DateTime.UtcNow);
        TemplateField existing = CreateTestField(version.Id);
        version.AddField(existing);
        TemplateField newField = CreateTestField(version.Id);

        // Act
        version.AddField(newField);

        // Assert
        Assert.AreEqual(2, version.Fields.Count);
        Assert.IsTrue(version.Fields.Contains(existing));
        Assert.IsTrue(version.Fields.Contains(newField));
    }

    // ===== Helpers =====

    private static TemplateField CreateTestField(Guid templateVersionId)
    {
        return TemplateField.Create(
            templateVersionId,
            new("Name"),
            new("Full Name"),
            FieldType.Text,
            true,
            0,
            new(true, 1, 100, null, null, null),
            new(1, 0, null, 12));
    }
}
