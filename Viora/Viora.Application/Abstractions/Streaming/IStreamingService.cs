namespace Viora.Application.Abstractions.Streaming;

public interface IStreamingService
{
    public IAsyncEnumerable<string> StreamResponseAsync(string prompt, string apiKey, string modelName, CancellationToken cancellationToken = default);
}
