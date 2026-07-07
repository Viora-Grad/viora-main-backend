using Viora.Domain.Archives;
using Viora.Domain.Archives.Internals;

namespace Viora.Test.Compenents.Domain.Archives;

/// <summary>
/// Unit tests for the Folder entity covering the Create factory method and Update method.
/// </summary>
[TestClass]
public sealed class FolderTests
{
    // ===== Create =====

    /// <summary>
    /// Verifies that Create with valid input returns a Folder with all properties correctly set.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsFolderWithCorrectProperties()
    {
        // Arrange
        Guid archiveId = Guid.NewGuid();
        Guid? parentFolderId = Guid.NewGuid();
        FolderName name = new("Radiology");
        FolderDescription description = new("X-ray and MRI records");
        FolderType type = FolderType.Normal;
        int order = 1;
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Folder folder = Folder.Create(archiveId, parentFolderId, name, description, type, order, createdAt);

        // Assert
        Assert.IsNotNull(folder);
        Assert.AreNotEqual(Guid.Empty, folder.Id);
        Assert.AreEqual(archiveId, folder.ArchiveId);
        Assert.AreEqual(parentFolderId, folder.ParentFolderId);
        Assert.AreEqual(name, folder.Name);
        Assert.AreEqual(description, folder.Description);
        Assert.AreEqual(type, folder.Type);
        Assert.AreEqual(order, folder.Order);
        Assert.IsFalse(folder.IsDeleted);
        Assert.AreEqual(createdAt, folder.CreatedAt);
    }

    /// <summary>
    /// Verifies that Create with null parentFolderId sets ParentFolderId to null.
    /// </summary>
    [TestMethod]
    public void Create_WithNullParentFolderId_ParentFolderIdIsNull()
    {
        // Arrange & Act
        Folder folder = Folder.Create(Guid.NewGuid(), null, new("Root Folder"), new("Root"), FolderType.Root, 0, DateTime.UtcNow);

        // Assert
        Assert.IsNull(folder.ParentFolderId);
    }

    /// <summary>
    /// Verifies that Create generates a new unique Id for each folder.
    /// </summary>
    [TestMethod]
    public void Create_CalledTwice_GeneratesDifferentIds()
    {
        // Arrange
        FolderType type = FolderType.Normal;

        // Act
        Folder folder1 = Folder.Create(Guid.NewGuid(), null, new("A"), new("D"), type, 0, DateTime.UtcNow);
        Folder folder2 = Folder.Create(Guid.NewGuid(), null, new("B"), new("E"), type, 1, DateTime.UtcNow);

        // Assert
        Assert.AreNotEqual(folder1.Id, folder2.Id);
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
        Folder folder = Folder.Create(archiveId, null, new("F"), new("D"), FolderType.Normal, 0, DateTime.UtcNow);

        // Assert
        Assert.AreEqual(archiveId, folder.ArchiveId);
    }

    /// <summary>
    /// Verifies that Create always sets IsDeleted to false.
    /// </summary>
    [TestMethod]
    public void Create_SetsIsDeletedToFalse()
    {
        // Arrange & Act
        Folder folder = Folder.Create(Guid.NewGuid(), null, new("F"), new("D"), FolderType.Normal, 0, DateTime.UtcNow);

        // Assert
        Assert.IsFalse(folder.IsDeleted);
    }

    /// <summary>
    /// Verifies that Create stores the folder order correctly.
    /// </summary>
    [TestMethod]
    public void Create_StoresOrderCorrectly()
    {
        // Arrange & Act
        Folder folder = Folder.Create(Guid.NewGuid(), null, new("F"), new("D"), FolderType.Normal, 42, DateTime.UtcNow);

        // Assert
        Assert.AreEqual(42, folder.Order);
    }

    /// <summary>
    /// Verifies that Create with FolderType.Root sets the type correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithRootType_SetsTypeToRoot()
    {
        // Arrange & Act
        Folder folder = Folder.Create(Guid.NewGuid(), null, new("Root"), new("Root Desc"), FolderType.Root, 0, DateTime.UtcNow);

        // Assert
        Assert.AreEqual(FolderType.Root, folder.Type);
    }

    /// <summary>
    /// Verifies that Create with FolderType.System sets the type correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithSystemType_SetsTypeToSystem()
    {
        // Arrange & Act
        Folder folder = Folder.Create(Guid.NewGuid(), null, new("Sys"), new("Sys Desc"), FolderType.System, 0, DateTime.UtcNow);

        // Assert
        Assert.AreEqual(FolderType.System, folder.Type);
    }

    // ===== Update =====

    /// <summary>
    /// Verifies that Update replaces the Name, Description, and Order properties.
    /// </summary>
    [TestMethod]
    public void Update_WithNewValues_ReplacesProperties()
    {
        // Arrange
        Folder folder = CreateTestFolder();
        FolderName newName = new("Updated Folder");
        FolderDescription newDesc = new("Updated Desc");
        int newOrder = 10;

        // Act
        folder.Update(newName, newDesc, newOrder);

        // Assert
        Assert.AreEqual(newName, folder.Name);
        Assert.AreEqual(newDesc, folder.Description);
        Assert.AreEqual(newOrder, folder.Order);
    }

    /// <summary>
    /// Verifies that Update can be called multiple times, keeping only the latest values.
    /// </summary>
    [TestMethod]
    public void Update_CalledMultipleTimes_KeepsLatestValues()
    {
        // Arrange
        Folder folder = CreateTestFolder();

        // Act
        folder.Update(new("First"), new("Desc 1"), 1);
        folder.Update(new("Second"), new("Desc 2"), 2);
        folder.Update(new("Third"), new("Desc 3"), 3);

        // Assert
        Assert.AreEqual("Third", folder.Name.Value);
        Assert.AreEqual("Desc 3", folder.Description.Value);
        Assert.AreEqual(3, folder.Order);
    }

    // ===== Helpers =====

    private static Folder CreateTestFolder()
    {
        return Folder.Create(
            Guid.NewGuid(),
            null,
            new("Test Folder"),
            new("Test Description"),
            FolderType.Normal,
            0,
            DateTime.UtcNow);
    }
}
