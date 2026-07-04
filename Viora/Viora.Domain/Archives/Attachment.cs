using System.Net.Mime;
using Viora.Domain.Archives.Internals;

namespace Viora.Domain.Archives;

public class Attachment
{
    public Guid RecordId { get; private set; }

    public FileName FileName { get; private set; }

    public BlobName BlobName { get; private set; }

    public ContentType ContentType { get; private set; }

    public long Size { get; private set; }

    public CheckSum Checksum { get; private set; }

    public DateTime UploadedAt { get; private set; }

    protected Attachment() { }

    private Attachment(
        Guid recordId,
        FileName fileName,
        BlobName blobName,
        ContentType contentType,
        long size,
        CheckSum checksum,
        DateTime uploadedAt)
    {
        RecordId = recordId;
        FileName = fileName;
        BlobName = blobName;
        ContentType = contentType;
        Size = size;
        Checksum = checksum;
        UploadedAt = uploadedAt;
    }

    public static Attachment Create(
        Guid recordId,
        FileName fileName,
        BlobName blobName,
        ContentType contentType,
        long size,
        CheckSum checksum,
        DateTime uploadedAt)
    {
        return new Attachment(
            recordId,
            fileName,
            blobName,
            contentType,
            size,
            checksum,
            uploadedAt);
    }
}
