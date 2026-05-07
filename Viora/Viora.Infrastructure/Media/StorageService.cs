using Viora.Application.Abstractions.Media;

namespace Viora.Infrastructure.Media;

public class StorageService : IStorageService
{
    private readonly string _basePath;

    public StorageService(IStorageConfiguration storage)
    {
        _basePath = Path.GetFullPath(storage.BasePath);
        if (!Directory.Exists(_basePath))
            throw new DirectoryNotFoundException($"Storage base path does not exist: {_basePath}");
    }
    public Task<Stream> GetFileStreamAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty.", nameof(key));

        var fullPath = Path.Combine(_basePath, key);

        var resolved = Path.GetFullPath(fullPath);
        if (!resolved.StartsWith(Path.GetFullPath(_basePath), StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Invalid key.");

        if (!File.Exists(resolved))
            throw new FileNotFoundException($"File not found: {key}", key);

        Stream stream = new FileStream(
            resolved,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        return Task.FromResult(stream);
    }
}
