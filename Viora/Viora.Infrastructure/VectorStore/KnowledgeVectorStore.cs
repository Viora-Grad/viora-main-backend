using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Viora.Application.AiRag.Abstractions;

namespace Viora.Infrastructure.VectorStore;

public sealed class KnowledgeVectorStore : IKnowledgeVectorStore
{
    private readonly QdrantClient _qdrant;
    private readonly ITextEmbeddingGenerationService _embedder;

    public KnowledgeVectorStore(QdrantClient qdrant, ITextEmbeddingGenerationService embedder)
    {
        _qdrant  = qdrant;
        _embedder = embedder;
    }

    // Ingestion
    public async Task IngestAsync(IReadOnlyList<KnowledgeChunk> chunks, CancellationToken ct = default)
    {
        await EnsureCollectionAsync(ct);

        const int batch = 20;
        for (var i = 0; i < chunks.Count; i += batch)
        {
            var slice      = chunks.Skip(i).Take(batch).ToList();
            var embeddings = await _embedder.GenerateEmbeddingsAsync(slice.Select(c => c.Content).ToList());

            var points = slice.Zip(embeddings, (chunk, emb) => new PointStruct
            {
                Id      = new PointId { Uuid = chunk.Id.ToString() },
                Vectors = emb.ToArray(),
                Payload =
                {
                    ["content"] = chunk.Content,
                    ["source"]  = chunk.Source,
                },
            }).ToList();

            await _qdrant.UpsertAsync(QdrantCollections.KnowledgeBase, points, cancellationToken: ct);
        }
    }

    // Search
    // Returns the top-K content strings most similar to the query.
    public async Task<IReadOnlyList<string>> SearchAsync(string query, int topK = 3, CancellationToken ct = default)
    {
        var embeddings = await _embedder.GenerateEmbeddingsAsync(new[] { query });
        var embedding = embeddings[0];

        var results = await _qdrant.SearchAsync(
            collectionName: QdrantCollections.KnowledgeBase,
            vector: embedding.ToArray(),
            limit: (ulong)topK,
            scoreThreshold: 0.60f,
            payloadSelector: true,
            cancellationToken: ct);

        return results
            .Select(r => r.Payload.TryGetValue("content", out var v) ? v.StringValue : string.Empty)
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
    }

    // Collection bootstrap
    private async Task EnsureCollectionAsync(CancellationToken ct)
    {
        if (await _qdrant.CollectionExistsAsync(QdrantCollections.KnowledgeBase, ct)) return;

        await _qdrant.CreateCollectionAsync(
            QdrantCollections.KnowledgeBase,
            new VectorParams
            {
                Size     = QdrantCollections.VectorDimension,
                Distance = Distance.Cosine,
            },
            cancellationToken: ct);
    }
}