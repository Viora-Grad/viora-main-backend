namespace Viora.Domain.AiRag.Intent;

public class DetectedIntent
{
    public ChatIntent Intent { get; set; }
    public string Confidence { get; set; } = IntentConfidence.High;
    public string? ExtractedQuery { get; set; }
    public string? ExtractedSymptoms { get; set; }
}