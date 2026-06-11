using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Viora.Domain.MedicalInquiries;

namespace Viora.Infrastructure.VectorStore;

public class SpecialtyVectorStore
{
    private readonly QdrantClient _qdrant;
    private readonly ITextEmbeddingGenerationService _embedding;

    public SpecialtyVectorStore(
        QdrantClient qdrant,
        ITextEmbeddingGenerationService embedding)
    {
        _qdrant = qdrant;
        _embedding = embedding;
    }

    public async Task IndexAsync(IEnumerable<MedicalInquiry> inquiries)
    {
        var collections = await _qdrant.ListCollectionsAsync();
        if (!collections.Any(c => c == QdrantCollections.Specialty))
        {
            await _qdrant.CreateCollectionAsync(
                QdrantCollections.Specialty,
                new VectorParams
                {
                    Size = QdrantCollections.VectorDimension,
                    Distance = Distance.Cosine,
                });
        }

        var list = inquiries.ToList();
        var texts = list.Select(s => s.EmbeddingText).ToList();
        var embeddings = await _embedding.GenerateEmbeddingsAsync(texts);

        var points = list.Zip(embeddings, (inquiry, vector) => new PointStruct
        {
            Id = new PointId { Uuid = inquiry.Id },
            Vectors = vector.ToArray(),
            Payload =
            {
                ["id"] = new Value { StringValue =  inquiry.Id},
                ["question"] = new Value { StringValue = inquiry.Question },
                ["specialty"] = new Value { StringValue = inquiry.Specialty },
            },
        }).ToList();

        await _qdrant.UpsertAsync(QdrantCollections.Specialty, points);
    }

    public async Task<List<MedicalInquiry>> SearchAsync(string query, int topK = 10)
    {
        var queryEmbeddings = await _embedding.GenerateEmbeddingsAsync(new[] { query });
        var queryVector = queryEmbeddings[0].ToArray();

        var results = await _qdrant.SearchAsync(
            QdrantCollections.Specialty,
            queryVector,
            limit: (ulong)topK,
            scoreThreshold: 0.60f);

        return results.Select(r => new MedicalInquiry()
        {
            Id = r.Payload["id"].StringValue,
            Question = r.Payload["question"].StringValue,
            Specialty = r.Payload["specialty"].StringValue,
        }).ToList();
    }
}