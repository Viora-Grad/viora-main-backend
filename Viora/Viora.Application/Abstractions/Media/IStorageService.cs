namespace Viora.Application.Abstractions.Media;

public interface IStorageService
{
    Task<Stream> GetFileStreamAsync(string key);
    Task SaveFileAsync(Stream stream, string key, CancellationToken cancellationToken = default);
}
