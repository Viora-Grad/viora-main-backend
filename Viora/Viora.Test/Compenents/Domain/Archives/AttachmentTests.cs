using System.Net.Mime;
using Viora.Domain.Archives;
using Viora.Domain.Archives.Internals;

namespace Viora.Test.Compenents.Domain.Archives;

/// <summary>
/// Unit tests for the Attachment entity covering the Create factory method.
/// </summary>
[TestClass]
public sealed class AttachmentTests
{
    // ===== Create =====

    /// <summary>
    /// Verifies that Create with valid input returns an Attachment with all properties correctly set.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsAttachmentWithCorrectProperties()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        FileName fileName = new("report.pdf");
        BlobName blobName = new("attachments/report.pdf");
        ContentType contentType = new("application/pdf");
        long size = 1024;
        CheckSum checksum = new("abc123def456");
        DateTime uploadedAt = DateTime.UtcNow;

        // Act
        Attachment attachment = Attachment.Create(recordId, fileName, blobName, contentType, size, checksum, uploadedAt);

        // Assert
        Assert.IsNotNull(attachment);
        Assert.AreEqual(recordId, attachment.RecordId);
        Assert.AreEqual(fileName, attachment.FileName);
        Assert.AreEqual(blobName, attachment.BlobName);
        Assert.AreEqual(contentType, attachment.ContentType);
        Assert.AreEqual(size, attachment.Size);
        Assert.AreEqual(checksum, attachment.Checksum);
        Assert.AreEqual(uploadedAt, attachment.UploadedAt);
    }

    /// <summary>
    /// Verifies that Create stores the file name correctly.
    /// </summary>
    [TestMethod]
    public void Create_StoresFileNameCorrectly()
    {
        // Arrange
        FileName fileName = new("xray_image.png");

        // Act
        Attachment attachment = Attachment.Create(
            Guid.NewGuid(),
            fileName,
            new("blob/xray.png"),
            new ContentType("image/png"),
            2048,
            new("checksum1"),
            DateTime.UtcNow);

        // Assert
        Assert.AreEqual(fileName, attachment.FileName);
    }

    /// <summary>
    /// Verifies that Create stores the blob name correctly.
    /// </summary>
    [TestMethod]
    public void Create_StoresBlobNameCorrectly()
    {
        // Arrange
        BlobName blobName = new("2024/01/report.pdf");

        // Act
        Attachment attachment = Attachment.Create(
            Guid.NewGuid(),
            new("report.pdf"),
            blobName,
            new ContentType("application/pdf"),
            500,
            new("chk"),
            DateTime.UtcNow);

        // Assert
        Assert.AreEqual(blobName, attachment.BlobName);
    }

    /// <summary>
    /// Verifies that Create stores the content type correctly.
    /// </summary>
    [TestMethod]
    public void Create_StoresContentTypeCorrectly()
    {
        // Arrange
        ContentType contentType = new(MediaTypeNames.Image.Jpeg);

        // Act
        Attachment attachment = Attachment.Create(
            Guid.NewGuid(),
            new("photo.jpg"),
            new("blob/photo.jpg"),
            contentType,
            4096,
            new("chk"),
            DateTime.UtcNow);

        // Assert
        Assert.AreEqual(contentType, attachment.ContentType);
    }

    /// <summary>
    /// Verifies that Create stores the size correctly.
    /// </summary>
    [TestMethod]
    public void Create_StoresSizeCorrectly()
    {
        // Arrange
        long size = 1048576;

        // Act
        Attachment attachment = Attachment.Create(
            Guid.NewGuid(),
            new("large_file.dat"),
            new("blob/large_file.dat"),
            new ContentType("application/octet-stream"),
            size,
            new("chk"),
            DateTime.UtcNow);

        // Assert
        Assert.AreEqual(size, attachment.Size);
    }

    /// <summary>
    /// Verifies that Create stores the checksum correctly.
    /// </summary>
    [TestMethod]
    public void Create_StoresChecksumCorrectly()
    {
        // Arrange
        CheckSum checksum = new("sha256hashvalue");

        // Act
        Attachment attachment = Attachment.Create(
            Guid.NewGuid(),
            new("file.txt"),
            new("blob/file.txt"),
            new ContentType("text/plain"),
            100,
            checksum,
            DateTime.UtcNow);

        // Assert
        Assert.AreEqual(checksum, attachment.Checksum);
    }

    /// <summary>
    /// Verifies that Create stores the uploaded at timestamp correctly.
    /// </summary>
    [TestMethod]
    public void Create_StoresUploadedAtCorrectly()
    {
        // Arrange
        DateTime uploadedAt = new(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        Attachment attachment = Attachment.Create(
            Guid.NewGuid(),
            new("file.txt"),
            new("blob/file.txt"),
            new ContentType("text/plain"),
            50,
            new("chk"),
            uploadedAt);

        // Assert
        Assert.AreEqual(uploadedAt, attachment.UploadedAt);
    }

    /// <summary>
    /// Verifies that Create with zero size works correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithZeroSize_WorksCorrectly()
    {
        // Arrange & Act
        Attachment attachment = Attachment.Create(
            Guid.NewGuid(),
            new("empty.txt"),
            new("blob/empty.txt"),
            new ContentType("text/plain"),
            0,
            new("chk"),
            DateTime.UtcNow);

        // Assert
        Assert.AreEqual(0, attachment.Size);
    }
}
