namespace Viora.Application.Abstractions.Media;

public interface IStorageService
{
    Stream GetFileStream(string key);
    Task SaveFileAsync(Stream stream, string key, CancellationToken cancellationToken = default);
    bool DeleteFile(string key);
}
