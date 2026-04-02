using Viora.Domain.Abstractions;

namespace Viora.Application.Medias;

public static class MediaErrors
{
    public static Error InvalidMediaSize(long sizeInBytes, long maximumAllowedSizeInBytes) =>
        new("InvalidMediaSize", $"The media size of {sizeInBytes} bytes exceeds the maximum allowed size of {maximumAllowedSizeInBytes} bytes.", ErrorCategory.Forbidden);
}
