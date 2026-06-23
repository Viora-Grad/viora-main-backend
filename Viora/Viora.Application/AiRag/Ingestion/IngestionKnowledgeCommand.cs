using Viora.Application.AiRag.Abstractions;

namespace Viora.Application.AiRag.Ingestion;

public sealed class IngestKnowledgeCommand
{
    private readonly IKnowledgeVectorStore _store;
    private readonly IKnowledgeChunkMapper _chunkMapper;

    public IngestKnowledgeCommand(IKnowledgeVectorStore store, IKnowledgeChunkMapper chunkMapper)
    {
        _store = store;
        _chunkMapper = chunkMapper;
    }

    public async Task ExecuteAsync(string markdownContent, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(markdownContent))
            throw new ArgumentException("Content cannot be empty.", nameof(markdownContent));

        var chunks = _chunkMapper.FromMarkdown(markdownContent);
        await _store.IngestAsync(chunks, ct);
    }
}