namespace Viora.Application.Abstractions.Media;

public sealed class MediaRequest
{
    public string FileName { get; }
    public string ContentType { get; }
    public long SizeBytes { get; }
    public Stream Content { get; }

    private MediaRequest(string fileName, string contentType, long sizeBytes, Stream content)
    {
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        Content = content;
    }

    /// <summary>
    /// creates an object in the allowed types of image/png, image/jpeg, image/webp
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="contentType"></param>
    /// <param name="sizeBytes"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static MediaRequest CreateImage(
        string fileName, string contentType, long sizeBytes, Stream content, long maxMediaSizeInBytes)
    {
        ValidateCommon(fileName, contentType, sizeBytes, content);

        var allowed = new[] { "image/png", "image/jpeg", "image/webp" };
        if (!allowed.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException(
                $"Content type '{contentType}' is not an allowed image type.",
                nameof(contentType));

        if (sizeBytes > maxMediaSizeInBytes)
            throw new ArgumentException($"Image exceeds {maxMediaSizeInBytes} limit.", nameof(sizeBytes));

        return new MediaRequest(fileName, contentType, sizeBytes, content);
    }

    /// <summary>
    /// creates document of the allowed types application/pdf, application/msword
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="contentType"></param>
    /// <param name="sizeBytes"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static MediaRequest CreateDocument(
        string fileName, string contentType, long sizeBytes, Stream content, long maxMediaSizeInBytes)
    {
        ValidateCommon(fileName, contentType, sizeBytes, content);

        var allowed = new[] { "application/pdf", "application/msword" };
        if (!allowed.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException(
                $"Content type '{contentType}' is not an allowed document type.",
                nameof(contentType));

        if (sizeBytes > maxMediaSizeInBytes)
            throw new ArgumentException($"Document exceeds {maxMediaSizeInBytes} limit.", nameof(sizeBytes));

        return new MediaRequest(fileName, contentType, sizeBytes, content);
    }

    private static void ValidateCommon(
        string fileName, string contentType, long sizeBytes, Stream content)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required.", nameof(fileName));
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type is required.", nameof(contentType));
        if (sizeBytes <= 0)
            throw new ArgumentException("File size must be positive.", nameof(sizeBytes));
        ArgumentNullException.ThrowIfNull(content);
        if (!content.CanRead)
            throw new ArgumentException("Stream must be readable.", nameof(content));
    }
}