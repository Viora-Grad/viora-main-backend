using Viora.Domain.Archives;
using Viora.Domain.Archives.Internals;

namespace Viora.Test.Compenents.Domain.Archives;

/// <summary>
/// Unit tests for the Template entity covering the Create factory method, Update method, and AddVersion method.
/// </summary>
[TestClass]
public sealed class TemplateTests
{
    // ===== Create =====

    /// <summary>
    /// Verifies that Create with valid input returns a Template with all properties correctly set.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsTemplateWithCorrectProperties()
    {
        // Arrange
        Guid archiveId = Guid.NewGuid();
        Guid folderId = Guid.NewGuid();
        TemplateName name = new("Patient Intake");
        TemplateDescription description = new("Standard intake form template");
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Template template = Template.Create(archiveId, folderId, name, description, createdAt);

        // Assert
        Assert.IsNotNull(template);
        Assert.AreNotEqual(Guid.Empty, template.Id);
        Assert.AreEqual(archiveId, template.ArchiveId);
        Assert.AreEqual(folderId, template.FolderId);
        Assert.AreEqual(name, template.Name);
        Assert.AreEqual(description, template.Description);
        Assert.AreEqual(0, template.CurrentVersion);
        Assert.AreEqual(0, template.Versions.Count);
        Assert.AreEqual(createdAt, template.CreatedAt);
    }

    /// <summary>
    /// Verifies that Create generates a new unique Id for each template.
    /// </summary>
    [TestMethod]
    public void Create_CalledTwice_GeneratesDifferentIds()
    {
        // Arrange
        Guid archiveId = Guid.NewGuid();
        Guid folderId = Guid.NewGuid();

        // Act
        Template t1 = Template.Create(archiveId, folderId, new("A"), new("D1"), DateTime.UtcNow);
        Template t2 = Template.Create(archiveId, folderId, new("B"), new("D2"), DateTime.UtcNow);

        // Assert
        Assert.AreNotEqual(t1.Id, t2.Id);
    }

    /// <summary>
    /// Verifies that Create with a specific archive ID links it correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithSpecificArchiveId_SetsArchiveIdCorrectly()
    {
        // Arrange
        Guid archiveId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        // Act
        Template template = Template.Create(archiveId, Guid.NewGuid(), new("T"), new("D"), DateTime.UtcNow);

        // Assert
        Assert.AreEqual(archiveId, template.ArchiveId);
    }

    /// <summary>
    /// Verifies that Create initializes CurrentVersion to 0.
    /// </summary>
    [TestMethod]
    public void Create_InitializesCurrentVersionToZero()
    {
        // Arrange & Act
        Template template = CreateTestTemplate();

        // Assert
        Assert.AreEqual(0, template.CurrentVersion);
    }

    /// <summary>
    /// Verifies that Create stores the template name correctly.
    /// </summary>
    [TestMethod]
    public void Create_StoresNameCorrectly()
    {
        // Arrange
        TemplateName name = new("Lab Results");

        // Act
        Template template = Template.Create(Guid.NewGuid(), Guid.NewGuid(), name, new("D"), DateTime.UtcNow);

        // Assert
        Assert.AreEqual(name, template.Name);
    }

    // ===== Update =====

    /// <summary>
    /// Verifies that Update replaces the Name and Description properties.
    /// </summary>
    [TestMethod]
    public void Update_WithNewValues_ReplacesProperties()
    {
        // Arrange
        Template template = CreateTestTemplate();
        TemplateName newName = new("Updated Template");
        TemplateDescription newDesc = new("Updated Description");

        // Act
        template.Update(newName, newDesc);

        // Assert
        Assert.AreEqual(newName, template.Name);
        Assert.AreEqual(newDesc, template.Description);
    }

    /// <summary>
    /// Verifies that Update can be called multiple times, keeping only the latest values.
    /// </summary>
    [TestMethod]
    public void Update_CalledMultipleTimes_KeepsLatestValues()
    {
        // Arrange
        Template template = CreateTestTemplate();

        // Act
        template.Update(new("First"), new("Desc 1"));
        template.Update(new("Second"), new("Desc 2"));
        template.Update(new("Third"), new("Desc 3"));

        // Assert
        Assert.AreEqual("Third", template.Name.Value);
        Assert.AreEqual("Desc 3", template.Description.Value);
    }

    // ===== AddVersion =====

    /// <summary>
    /// Verifies that AddVersion adds a version and updates CurrentVersion.
    /// </summary>
    [TestMethod]
    public void AddVersion_SingleVersion_AddsToCollectionAndUpdatesCurrentVersion()
    {
        // Arrange
        Template template = CreateTestTemplate();
        TemplateVersion version = TemplateVersion.Create(template.Id, 1, DateTime.UtcNow);

        // Act
        template.AddVersion(version);

        // Assert
        Assert.AreEqual(1, template.Versions.Count);
        Assert.IsTrue(template.Versions.Contains(version));
        Assert.AreEqual(1, template.CurrentVersion);
    }

    /// <summary>
    /// Verifies that AddVersion with multiple versions updates CurrentVersion to the latest.
    /// </summary>
    [TestMethod]
    public void AddVersion_MultipleVersions_UpdatesCurrentVersionToLatest()
    {
        // Arrange
        Template template = CreateTestTemplate();
        TemplateVersion v1 = TemplateVersion.Create(template.Id, 1, DateTime.UtcNow);
        TemplateVersion v2 = TemplateVersion.Create(template.Id, 2, DateTime.UtcNow);
        TemplateVersion v3 = TemplateVersion.Create(template.Id, 3, DateTime.UtcNow);

        // Act
        template.AddVersion(v1);
        template.AddVersion(v2);
        template.AddVersion(v3);

        // Assert
        Assert.AreEqual(3, template.Versions.Count);
        Assert.AreEqual(3, template.CurrentVersion);
    }

    /// <summary>
    /// Verifies that AddVersion does not remove previously added versions.
    /// </summary>
    [TestMethod]
    public void AddVersion_MultipleVersions_KeepsAllVersions()
    {
        // Arrange
        Template template = CreateTestTemplate();
        TemplateVersion v1 = TemplateVersion.Create(template.Id, 1, DateTime.UtcNow);
        TemplateVersion v2 = TemplateVersion.Create(template.Id, 2, DateTime.UtcNow);

        // Act
        template.AddVersion(v1);
        template.AddVersion(v2);

        // Assert
        Assert.AreEqual(2, template.Versions.Count);
        Assert.IsTrue(template.Versions.Contains(v1));
        Assert.IsTrue(template.Versions.Contains(v2));
    }

    // ===== Helpers =====

    private static Template CreateTestTemplate()
    {
        return Template.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new("Test Template"),
            new("Test Description"),
            DateTime.UtcNow);
    }
}
