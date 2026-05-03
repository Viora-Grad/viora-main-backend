namespace Viora.Application.Abstractions.Media;

internal interface IStorageService
{
    public Task<Stream> GetFileStreamAsync(string key);
}
