using Viora.Domain.Abstractions;
using Viora.Domain.Medias.Internals;

namespace Viora.Domain.Medias;

public sealed class MediaFile : Entity
{
    // If the image is a profile picture for customer and such nullable else it relates to org and is conumed towards its quota
    public Guid? OrganizationId { get; private set; } = null;
    public Name Name { get; private set; } = default!;
    public MimeType MimeType { get; private set; } = default!;
    public long SizeInBytes { get; private set; }
    public MediaKey Key { get; private set; } = default!;
    public DateTime UploadedAtUtc { get; private set; }

    public MediaType CategoryType => MimeType.Value switch
    {
        "image/jpeg" or "image/png" or "image/gif" or "image/webp" => MediaType.Image,
        "audio/mpeg" or "audio/wav" => MediaType.Audio,
        "video/mp4" => MediaType.Video,
        "application/pdf" or "application/msword" => MediaType.Document,
        _ => MediaType.Binary
    };

    private MediaFile(Guid id, Name name, long sizeInBytes, MediaKey key, MimeType type, DateTime uploadedAtUtc, Guid? organizationId) : base(id)
    {
        Name = name;
        MimeType = type;
        SizeInBytes = sizeInBytes;
        Key = key;
        UploadedAtUtc = uploadedAtUtc;
        OrganizationId = organizationId;
    }

    private MediaFile() : base() { } // for EfCore

    public static Result<MediaFile> Create(string name, long sizeInBytes, string key, string mimeType, DateTime uploadTimeUtc, long maximumMediaSizeInBytes, Guid? organizationId)
    {
        var media = new MediaFile(Guid.NewGuid(), name, sizeInBytes, key, mimeType, uploadTimeUtc, organizationId);

        if (media.SizeInBytes > maximumMediaSizeInBytes)
            return Result.Failure<MediaFile>(MediaErrors.InvalidMediaSize(sizeInBytes, maximumMediaSizeInBytes));

        return Result.Success(media);
    }
}