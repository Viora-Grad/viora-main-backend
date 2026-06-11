namespace Viora.Infrastructure.VectorStore;

public class QdrantCollections
{
    public const string Faq = "viora_faq";
    public const string Specialty = "viora_medical_specialties";

    /// Gemini text-embedding-004 outputs 768-dimension vectors
    public const ulong VectorDimension = 768;
}