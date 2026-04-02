using Viora.Application.Medias.Internals;
using Viora.Domain.Abstractions;

namespace Viora.Application.Medias;

public sealed class MediaFile : Entity
{
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

    private MediaFile(Guid id, long sizeInBytes, MediaKey key, MimeType type, DateTime uploadedAtUtc) : base(id)
    {
        MimeType = type;
        SizeInBytes = sizeInBytes;
        Key = key;
        UploadedAtUtc = uploadedAtUtc;
    }

    private MediaFile() : base() { } // for EfCore

    public static Result<MediaFile> Create(long sizeInBytes, string key, string mimeType, DateTime uploadTimeUtc, long maximumMediaSizeInBytes)
    {
        var media = new MediaFile(Guid.NewGuid(), sizeInBytes, key, mimeType, uploadTimeUtc);

        if (media.SizeInBytes > maximumMediaSizeInBytes)
            return Result.Failure<MediaFile>(MediaErrors.InvalidMediaSize(sizeInBytes, maximumMediaSizeInBytes));

        return Result.Success(media);
    }
}