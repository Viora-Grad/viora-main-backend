using Google.GenAI;
using System.Runtime.CompilerServices;
using Viora.Application.Abstractions.Streaming;

internal class GeminiStreamingService : IStreamingService
{
    public async IAsyncEnumerable<string> StreamResponseAsync(string prompt, string apiKey, string modelName, [EnumeratorCancellation] CancellationToken ct = default)
    {
        Client _client = new(apiKey: apiKey);

        var stream = _client.Models.GenerateContentStreamAsync(
                model: modelName,
                contents: prompt,
                cancellationToken: ct
            );

        await foreach (var chunk in stream.WithCancellation(ct))
        {
            var text = chunk.Candidates?[0]?.Content?.Parts?[0]?.Text;
            if (!string.IsNullOrEmpty(text))
            {
                yield return text;
            }
        }
    }
}