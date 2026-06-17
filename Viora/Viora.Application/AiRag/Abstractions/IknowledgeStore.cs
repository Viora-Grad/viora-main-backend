// Application layer cannot reference Infrastructure directly.
// KnowledgeHandler depends on this interface, not on KnowledgeVectorStore.
// Infrastructure registers KnowledgeVectorStore as the implementation.
namespace Viora.Application.AiRag.Abstractions;

public interface IKnowledgeVectorStore
{
    /// <summary>
    /// Searches viora_knowledge Qdrant collection.
    /// Returns top-K chunk content strings ordered by similarity.
    /// </summary>
    Task<IReadOnlyList<string>> SearchAsync(string query, int topK = 3, CancellationToken ct = default);

    /// <summary>
    /// Ingests pre-built chunks into viora_knowledge Qdrant collection.
    /// Called only by IngestKnowledgeCommand via IKnowledgeIngestionService.
    /// </summary>
    Task IngestAsync(IReadOnlyList<KnowledgeChunk> chunks, CancellationToken ct = default);
}

/// <summary>
/// Chunk passed from ingestion command to vector store.
/// Defined here so Application layer owns the contract fully.
/// </summary>
public sealed record KnowledgeChunk(Guid Id, string Content, string Source);