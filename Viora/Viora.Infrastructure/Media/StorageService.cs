using Viora.Application.Abstractions.Media;

namespace Viora.Infrastructure.Media;

public class StorageService(IStorageSettings storage) : IStorageService
{
    private readonly string _basePath = Path.GetFullPath(storage.BasePath);

    public Stream GetFileStream(string key)
    {
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty.", nameof(key));

        var resolved = ResolvePath(key);

        if (!File.Exists(resolved))
            throw new FileNotFoundException($"File not found: {key}", key);

        Stream stream = new FileStream(
            resolved,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        return stream;
    }

    public async Task SaveFileAsync(Stream stream, string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty.", nameof(key));

        var resolved = ResolvePath(key);

        var directory = Path.GetDirectoryName(resolved)!;
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        await using var fileStream = new FileStream(
            resolved,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            useAsync: true);

        await stream.CopyToAsync(fileStream, cancellationToken);
    }
    public bool DeleteFile(string key)
    {
        if (!Directory.Exists(_basePath))
            return false;

        File.Delete(ResolvePath(key));
        return true;
    }

    private string ResolvePath(string key)
    {
        var fullPath = Path.Combine(_basePath, key);
        var resolved = Path.GetFullPath(fullPath);

        if (!resolved.StartsWith(Path.GetFullPath(_basePath), StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Invalid key.");

        return resolved;
    }
}
