namespace Viora.Domain.MedicalInquiries;

public class MedicalInquiry
{
    public string Id { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;

    /// Pre-built text used for embedding: "Name: {Name}. {Description}"
    public string EmbeddingText => $"Specialty: {Specialty}. {Question}";
}