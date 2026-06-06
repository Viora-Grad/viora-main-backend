using Viora.Domain.Abstractions;

namespace Viora.Domain.Feedbacks.Events;

/// <summary>
/// Used to update the overall organization rating related to the branch
/// </summary>
/// <param name="BranchId"></param>
public sealed record FeedbackSubmittedEvent(Guid BranchId) : IDomainEvent;
