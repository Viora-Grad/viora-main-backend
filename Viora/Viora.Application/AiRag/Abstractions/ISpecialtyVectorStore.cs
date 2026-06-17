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
    /// Ingests a batch of specialty names into Qdrant. Idempotent: if a name already exists, it is overwritten.
    /// </summary>
    Task IndexAsync(IEnumerable<MedicalInquiry> specialtyNames, CancellationToken ct = default);
}