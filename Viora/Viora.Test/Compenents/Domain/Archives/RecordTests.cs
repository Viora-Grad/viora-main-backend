using System.Net.Mime;
using Viora.Domain.Archives;
using Viora.Domain.Archives.Internals;

namespace Viora.Test.Compenents.Domain.Archives;

/// <summary>
/// Unit tests for the Record entity covering the Create factory method and collection methods.
/// </summary>
[TestClass]
public sealed class RecordTests
{
    // ===== Create =====

    /// <summary>
    /// Verifies that Create with valid input returns a Record with all properties correctly set.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsRecordWithCorrectProperties()
    {
        // Arrange
        Guid archiveId = Guid.NewGuid();
        Guid folderId = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();
        Guid? appointmentId = Guid.NewGuid();
        Guid templateId = Guid.NewGuid();
        Guid templateVersionId = Guid.NewGuid();
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Record record = Record.Create(archiveId, folderId, customerId, appointmentId, templateId, templateVersionId, createdAt);

        // Assert
        Assert.IsNotNull(record);
        Assert.AreNotEqual(Guid.Empty, record.Id);
        Assert.AreEqual(archiveId, record.ArchiveId);
        Assert.AreEqual(folderId, record.FolderId);
        Assert.AreEqual(customerId, record.CustomerId);
        Assert.AreEqual(appointmentId, record.AppointmentId);
        Assert.AreEqual(templateId, record.TemplateId);
        Assert.AreEqual(templateVersionId, record.TemplateVersionId);
        Assert.AreEqual(createdAt, record.CreatedAt);
        Assert.IsNull(record.UpdatedAt);
        Assert.AreEqual(0, record.Values.Count);
        Assert.AreEqual(0, record.Attachments.Count);
    }

    /// <summary>
    /// Verifies that Create with null appointmentId sets AppointmentId to null.
    /// </summary>
    [TestMethod]
    public void Create_WithNullAppointmentId_AppointmentIdIsNull()
    {
        // Arrange & Act
        Record record = Record.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

        // Assert
        Assert.IsNull(record.AppointmentId);
    }

    /// <summary>
    /// Verifies that Create generates a new unique Id for each record.
    /// </summary>
    [TestMethod]
    public void Create_CalledTwice_GeneratesDifferentIds()
    {
        // Arrange
        Guid archiveId = Guid.NewGuid();
        Guid folderId = Guid.NewGuid();

        // Act
        Record record1 = Record.Create(archiveId, folderId, Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        Record record2 = Record.Create(archiveId, folderId, Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

        // Assert
        Assert.AreNotEqual(record1.Id, record2.Id);
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
        Record record = Record.Create(archiveId, Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

        // Assert
        Assert.AreEqual(archiveId, record.ArchiveId);
    }

    /// <summary>
    /// Verifies that Create initializes UpdatedAt to null.
    /// </summary>
    [TestMethod]
    public void Create_InitializesUpdatedAtToNull()
    {
        // Arrange & Act
        Record record = CreateTestRecord();

        // Assert
        Assert.IsNull(record.UpdatedAt);
    }

    // ===== AddValue =====

    /// <summary>
    /// Verifies that AddValue adds a value to the Values collection and sets UpdatedAt.
    /// </summary>
    [TestMethod]
    public void AddValue_SingleValue_AddsToCollectionAndSetsUpdatedAt()
    {
        // Arrange
        Record record = CreateTestRecord();
        RecordFieldValue value = new(Guid.NewGuid(), "Name", FieldType.Text, "John");

        // Act
        record.AddValue(value);

        // Assert
        Assert.AreEqual(1, record.Values.Count);
        Assert.IsTrue(record.Values.Contains(value));
        Assert.IsNotNull(record.UpdatedAt);
    }

    /// <summary>
    /// Verifies that AddValue with multiple values adds all to the collection.
    /// </summary>
    [TestMethod]
    public void AddValue_MultipleValues_AddsAllToCollection()
    {
        // Arrange
        Record record = CreateTestRecord();
        RecordFieldValue value1 = new(Guid.NewGuid(), "Name", FieldType.Text, "John");
        RecordFieldValue value2 = new(Guid.NewGuid(), "Age", FieldType.Number, 30);
        RecordFieldValue value3 = new(Guid.NewGuid(), "Active", FieldType.Boolean, true);

        // Act
        record.AddValue(value1);
        record.AddValue(value2);
        record.AddValue(value3);

        // Assert
        Assert.AreEqual(3, record.Values.Count);
    }

    /// <summary>
    /// Verifies that AddValue with null value object is added to the collection.
    /// </summary>
    [TestMethod]
    public void AddValue_NullValue_AddedToCollection()
    {
        // Arrange
        Record record = CreateTestRecord();
        RecordFieldValue value = new(Guid.NewGuid(), "Notes", FieldType.Text, null);

        // Act
        record.AddValue(value);

        // Assert
        Assert.AreEqual(1, record.Values.Count);
        Assert.IsNull(record.Values.First().Value);
    }

    // ===== ClearValues =====

    /// <summary>
    /// Verifies that ClearValues removes all values and sets UpdatedAt.
    /// </summary>
    [TestMethod]
    public void ClearValues_WithValues_RemovesAllAndSetsUpdatedAt()
    {
        // Arrange
        Record record = CreateTestRecord();
        record.AddValue(new(Guid.NewGuid(), "Name", FieldType.Text, "John"));
        record.AddValue(new(Guid.NewGuid(), "Age", FieldType.Number, 30));

        // Act
        record.ClearValues();

        // Assert
        Assert.AreEqual(0, record.Values.Count);
        Assert.IsNotNull(record.UpdatedAt);
    }

    /// <summary>
    /// Verifies that ClearValues on an empty collection does not throw.
    /// </summary>
    [TestMethod]
    public void ClearValues_EmptyCollection_DoesNotThrow()
    {
        // Arrange
        Record record = CreateTestRecord();

        // Act & Assert
        try
        {
            record.ClearValues();
            Assert.IsTrue(true);
        }
        catch (Exception)
        {
            Assert.Fail("Expected no exception but one was thrown.");
        }
    }

    // ===== AddAttachment =====

    /// <summary>
    /// Verifies that AddAttachment adds an attachment to the Attachments collection and sets UpdatedAt.
    /// </summary>
    [TestMethod]
    public void AddAttachment_SingleAttachment_AddsToCollectionAndSetsUpdatedAt()
    {
        // Arrange
        Record record = CreateTestRecord();
        Attachment attachment = Attachment.Create(
            record.Id,
            new("report.pdf"),
            new("blob/report.pdf"),
            new ContentType("application/pdf"),
            1024,
            new("abc123"),
            DateTime.UtcNow);

        // Act
        record.AddAttachment(attachment);

        // Assert
        Assert.AreEqual(1, record.Attachments.Count);
        Assert.IsTrue(record.Attachments.Contains(attachment));
        Assert.IsNotNull(record.UpdatedAt);
    }

    /// <summary>
    /// Verifies that AddAttachment with multiple attachments adds all to the collection.
    /// </summary>
    [TestMethod]
    public void AddAttachment_MultipleAttachments_AddsAllToCollection()
    {
        // Arrange
        Record record = CreateTestRecord();
        Guid recordId = record.Id;
        Attachment a1 = Attachment.Create(recordId, new("f1.txt"), new("b1"), new ContentType("text/plain"), 100, new("c1"), DateTime.UtcNow);
        Attachment a2 = Attachment.Create(recordId, new("f2.txt"), new("b2"), new ContentType("text/plain"), 200, new("c2"), DateTime.UtcNow);

        // Act
        record.AddAttachment(a1);
        record.AddAttachment(a2);

        // Assert
        Assert.AreEqual(2, record.Attachments.Count);
    }

    // ===== Helpers =====

    private static Record CreateTestRecord()
    {
        return Record.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow);
    }
}
