using Viora.Domain.Abstractions;
using Viora.Domain.Medias.Internals;

namespace Viora.Domain.Medias;

public sealed class MediaFile : Entity
{
    public Name Name { get; set; } = default!;
    public MimeType MimeType { get; private set; } = default!;
    public long SizeInBytes { get; private set; }
    public MediaKey Key { get; private set; } = default!;
    public DateTime UploadedAtUtc { get; private set; }

    public MediaType CategoryType => MimeType.Value switch
    {
        "image/jpeg" or "image/png" or "image/gif" => MediaType.Image,
        "audio/mpeg" or "audio/wav" => MediaType.Audio,
        "video/mp4" => MediaType.Video,
        _ => MediaType.Binary
    };

    private MediaFile(Guid id, Name name, long sizeInBytes, MediaKey key, MimeType type, DateTime uploadedAtUtc) : base(id)
    {
        Name = name;
        MimeType = type;
        SizeInBytes = sizeInBytes;
        Key = key;
        UploadedAtUtc = uploadedAtUtc;
    }

    private MediaFile() : base() { } // for EfCore

    public static Result<MediaFile> Create(string name, long sizeInBytes, string key, string mimeType, DateTime uploadTimeUtc, long maximumMediaSizeInBytes)
    {
        var media = new MediaFile(Guid.NewGuid(), name, sizeInBytes, key, mimeType, uploadTimeUtc);

        if (media.SizeInBytes > maximumMediaSizeInBytes)
            return Result.Failure<MediaFile>(MediaErrors.InvalidMediaSize(sizeInBytes, maximumMediaSizeInBytes));

        return Result.Success(media);
    }
}