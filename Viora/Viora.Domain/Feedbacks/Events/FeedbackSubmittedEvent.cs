namespace Viora.Domain.Feedbacks.Events;

/// <summary>
/// Used to update the overall organization rating related to the branch
/// </summary>
/// <param name="BranchId"></param>
public record FeedbackSubmittedEvent(Guid BranchId);
