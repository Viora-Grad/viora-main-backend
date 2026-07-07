using Viora.Domain.Archives;
using Viora.Domain.Archives.Internals;

namespace Viora.Test.Compenents.Domain.Archives;

/// <summary>
/// Unit tests for the Archive entity covering the Create factory method and Update method.
/// </summary>
[TestClass]
public sealed class ArchiveTests
{
    // ===== Create =====

    /// <summary>
    /// Verifies that Create with valid input returns an Archive with all properties correctly set.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsArchiveWithCorrectProperties()
    {
        // Arrange
        Guid organizationId = Guid.NewGuid();
        ArchiveName name = new("Medical Records Archive");
        ArchiveDescription description = new("Stores patient medical records");
        ArchiveSettings settings = new(true, true, false, true);
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Archive archive = Archive.Create(organizationId, name, description, settings, createdAt);

        // Assert
        Assert.IsNotNull(archive);
        Assert.AreNotEqual(Guid.Empty, archive.Id);
        Assert.AreEqual(organizationId, archive.OrganizationId);
        Assert.AreEqual(name, archive.Name);
        Assert.AreEqual(description, archive.Description);
        Assert.AreEqual(settings, archive.Setting);
        Assert.AreEqual(createdAt, archive.CreatedAt);
        Assert.AreNotEqual(Guid.Empty, archive.RootFolder);
    }

    /// <summary>
    /// Verifies that Create generates a new unique Id for each archive.
    /// </summary>
    [TestMethod]
    public void Create_CalledTwice_GeneratesDifferentIds()
    {
        // Arrange
        ArchiveSettings settings = new(false, false, false, false);

        // Act
        Archive archive1 = Archive.Create(Guid.NewGuid(), new("Archive A"), new("Desc A"), settings, DateTime.UtcNow);
        Archive archive2 = Archive.Create(Guid.NewGuid(), new("Archive B"), new("Desc B"), settings, DateTime.UtcNow);

        // Assert
        Assert.AreNotEqual(archive1.Id, archive2.Id);
    }

    /// <summary>
    /// Verifies that Create generates a new unique RootFolder for each archive.
    /// </summary>
    [TestMethod]
    public void Create_CalledTwice_GeneratesDifferentRootFolders()
    {
        // Arrange
        ArchiveSettings settings = new(false, false, false, false);

        // Act
        Archive archive1 = Archive.Create(Guid.NewGuid(), new("A1"), new("D1"), settings, DateTime.UtcNow);
        Archive archive2 = Archive.Create(Guid.NewGuid(), new("A2"), new("D2"), settings, DateTime.UtcNow);

        // Assert
        Assert.AreNotEqual(archive1.RootFolder, archive2.RootFolder);
    }

    /// <summary>
    /// Verifies that Create with a specific organization ID links it correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithSpecificOrganizationId_SetsOrganizationIdCorrectly()
    {
        // Arrange
        Guid organizationId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        ArchiveSettings settings = new(true, false, true, false);

        // Act
        Archive archive = Archive.Create(organizationId, new("Test"), new("Test Desc"), settings, DateTime.UtcNow);

        // Assert
        Assert.AreEqual(organizationId, archive.OrganizationId);
    }

    /// <summary>
    /// Verifies that Create stores the archive name correctly.
    /// </summary>
    [TestMethod]
    public void Create_StoresNameCorrectly()
    {
        // Arrange
        ArchiveName name = new("Patient Files");
        ArchiveSettings settings = new(false, true, false, true);

        // Act
        Archive archive = Archive.Create(Guid.NewGuid(), name, new("Desc"), settings, DateTime.UtcNow);

        // Assert
        Assert.AreEqual(name, archive.Name);
    }

    /// <summary>
    /// Verifies that Create stores the settings correctly.
    /// </summary>
    [TestMethod]
    public void Create_StoresSettingsCorrectly()
    {
        // Arrange
        ArchiveSettings settings = new(EnableVersioning: true, EnableAttachments: true, EnableExport: false, EnableAudit: true);

        // Act
        Archive archive = Archive.Create(Guid.NewGuid(), new("A"), new("D"), settings, DateTime.UtcNow);

        // Assert
        Assert.IsTrue(archive.Setting.EnableVersioning);
        Assert.IsTrue(archive.Setting.EnableAttachments);
        Assert.IsFalse(archive.Setting.EnableExport);
        Assert.IsTrue(archive.Setting.EnableAudit);
    }

    // ===== Update =====

    /// <summary>
    /// Verifies that Update replaces the Name, Description, and Setting properties.
    /// </summary>
    [TestMethod]
    public void Update_WithNewValues_ReplacesProperties()
    {
        // Arrange
        ArchiveSettings initialSettings = new(false, false, false, false);
        Archive archive = Archive.Create(Guid.NewGuid(), new("Old Name"), new("Old Desc"), initialSettings, DateTime.UtcNow);
        ArchiveName newName = new("New Name");
        ArchiveDescription newDesc = new("New Desc");
        ArchiveSettings newSettings = new(true, true, true, true);

        // Act
        archive.Update(newName, newDesc, newSettings);

        // Assert
        Assert.AreEqual(newName, archive.Name);
        Assert.AreEqual(newDesc, archive.Description);
        Assert.AreEqual(newSettings, archive.Setting);
    }

    /// <summary>
    /// Verifies that Update can be called multiple times, keeping only the latest values.
    /// </summary>
    [TestMethod]
    public void Update_CalledMultipleTimes_KeepsLatestValues()
    {
        // Arrange
        ArchiveSettings settings = new(false, false, false, false);
        Archive archive = Archive.Create(Guid.NewGuid(), new("A"), new("D"), settings, DateTime.UtcNow);

        // Act
        archive.Update(new("First"), new("First Desc"), settings);
        archive.Update(new("Second"), new("Second Desc"), settings);
        archive.Update(new("Third"), new("Third Desc"), settings);

        // Assert
        Assert.AreEqual("Third", archive.Name.Value);
        Assert.AreEqual("Third Desc", archive.Description.Value);
    }

    // ===== Helpers =====

    private static Archive CreateTestArchive()
    {
        return Archive.Create(
            Guid.NewGuid(),
            new("Test Archive"),
            new("Test Description"),
            new(true, true, false, true),
            DateTime.UtcNow);
    }
}
