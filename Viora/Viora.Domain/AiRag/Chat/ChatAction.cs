namespace Viora.Domain.AiRag.Chat;

public class ChatAction
{
    public string Label { get; set; } = string.Empty;
    public string ActionType { get; set; } = "specialty";
    public string Specialty { get; set; } = string.Empty;
    public string? OrgName { get; set; }
    public string? Country { get; set; }
    public string? ServiceType { get; set; }
    public double? MinRating { get; set; }
}
