namespace Viora.Application.Abstractions.Media;

public interface IStorageService
{
    public Task<Stream> GetFileStreamAsync(string key);
}
