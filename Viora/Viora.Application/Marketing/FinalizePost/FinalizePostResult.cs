namespace Viora.Application.Marketing.FinalizePost;

// FacebookPostId is null here — finalize only prepares the draft locally; the live post id is assigned at publish.
public sealed record FinalizePostResult(
    Guid ChatId,
    string? FacebookPostId,
    string? Title,
    string AssistantMessage);
