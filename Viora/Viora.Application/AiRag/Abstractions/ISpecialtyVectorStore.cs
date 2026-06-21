using Viora.Domain.MedicalInquiries;

namespace Viora.Application.AiRag.Abstractions;

public interface ISpecialtyVectorStore
{
    /// <summary>
    /// Searches medical_specialty Qdrant collection.
    /// Returns top-K distinct specialty names ordered by similarity.
    /// </summary>
    Task<List<MedicalInquiry>> SearchAsync(string query, int topK = 10, CancellationToken ct = default);

    /// <summary>
    /// Streams specialty inquiries into Qdrant in batches. Idempotent: existing ids are overwritten.
    /// </summary>
    Task IndexAsync(IAsyncEnumerable<MedicalInquiry> inquiries, CancellationToken ct = default);
}