using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Viora.Application.AiRag.Abstractions;
using Viora.Domain.MedicalInquiries;

namespace Viora.Infrastructure.VectorStore;

public class SpecialtyVectorStore : ISpecialtyVectorStore
{
    // Number of inquiries embedded + upserted per round-trip. Keeps the ONNX
    // input tensor small (dynamic padding to the longest item in the batch)
    // and bounds memory regardless of how large the source file is.
    private const int BatchSize = 256;

    // Restored after a bulk load so the HNSW index builds once at the end.
    private const ulong DefaultIndexingThreshold = 20_000;

    private readonly QdrantClient _qdrant;
    private readonly ITextEmbeddingGenerationService _embedding;
    private readonly ILogger<SpecialtyVectorStore> _logger;

    public SpecialtyVectorStore(
        QdrantClient qdrant,
        ITextEmbeddingGenerationService embedding,
        ILogger<SpecialtyVectorStore> logger)
    {
        _qdrant = qdrant;
        _embedding = embedding;
        _logger = logger;
    }

    public async Task IndexAsync(IAsyncEnumerable<MedicalInquiry> inquiries, CancellationToken ct = default)
    {
        await EnsureCollectionAsync(ct);

        // Pause HNSW indexing while streaming points in; restore afterwards so
        // the graph is built once rather than incrementally on every upsert.
        await SetIndexingThresholdAsync(0, ct);
        try
        {
            var batch = new List<MedicalInquiry>(BatchSize);
            Task? pendingUpsert = null;
            var total = 0;

            await foreach (var inquiry in inquiries.WithCancellation(ct))
            {
                batch.Add(inquiry);
                if (batch.Count < BatchSize) continue;

                pendingUpsert = await FlushAsync(batch, pendingUpsert, ct);
                total += batch.Count;
                batch = new List<MedicalInquiry>(BatchSize);

                if (total % (BatchSize * 20) == 0)
                    _logger.LogInformation("Specialty ingestion progress: {Total} inquiries", total);
            }

            if (batch.Count > 0)
            {
                pendingUpsert = await FlushAsync(batch, pendingUpsert, ct);
                total += batch.Count;
            }

            if (pendingUpsert is not null) await pendingUpsert;

            _logger.LogInformation("Specialty ingestion complete: {Total} inquiries", total);
        }
        finally
        {
            await SetIndexingThresholdAsync(DefaultIndexingThreshold, ct);
        }
    }

    // Embeds the current batch while the previous batch's upsert is still in
    // flight (CPU embedding overlaps the network round-trip), then awaits the
    // previous upsert for backpressure before firing the next one.
    private async Task<Task> FlushAsync(List<MedicalInquiry> batch, Task? pendingUpsert, CancellationToken ct)
    {
        var points = await EmbedBatchAsync(batch, ct);

        if (pendingUpsert is not null) await pendingUpsert;

        return _qdrant.UpsertAsync(QdrantCollections.Specialty, points, wait: false, cancellationToken: ct);
    }

    private async Task<List<PointStruct>> EmbedBatchAsync(List<MedicalInquiry> batch, CancellationToken ct)
    {
        var texts = batch.Select(s => s.EmbeddingText).ToList();
        var embeddings = await _embedding.GenerateEmbeddingsAsync(texts, cancellationToken: ct);

        return batch.Zip(embeddings, (inquiry, vector) => new PointStruct
        {
            Id = new PointId { Uuid = inquiry.Id },
            Vectors = vector.ToArray(),
            Payload =
            {
                ["id"] = inquiry.Id,
                ["question"] = inquiry.Question,
                ["specialty"] = inquiry.Specialty,
            },
        }).ToList();
    }

    public async Task<List<MedicalInquiry>> SearchAsync(string query, int topK = 10, CancellationToken ct = default)
    {
        var queryEmbeddings = await _embedding.GenerateEmbeddingsAsync(new[] { query }, cancellationToken: ct);
        var queryVector = queryEmbeddings[0].ToArray();

        var results = await _qdrant.SearchAsync(
            QdrantCollections.Specialty,
            queryVector,
            limit: (ulong)topK,
            scoreThreshold: 0.60f,
            cancellationToken: ct);

        return results.Select(r => new MedicalInquiry()
        {
            Id = r.Payload["id"].StringValue,
            Question = r.Payload["question"].StringValue,
            Specialty = r.Payload["specialty"].StringValue,
        }).ToList();
    }

    private async Task EnsureCollectionAsync(CancellationToken ct)
    {
        if (await _qdrant.CollectionExistsAsync(QdrantCollections.Specialty, ct)) return;

        await _qdrant.CreateCollectionAsync(
            QdrantCollections.Specialty,
            new VectorParams
            {
                Size = QdrantCollections.VectorDimension,
                Distance = Distance.Cosine,
            },
            cancellationToken: ct);
    }

    private Task SetIndexingThresholdAsync(ulong threshold, CancellationToken ct) =>
        _qdrant.UpdateCollectionAsync(
            QdrantCollections.Specialty,
            optimizersConfig: new OptimizersConfigDiff { IndexingThreshold = threshold },
            cancellationToken: ct);
}
