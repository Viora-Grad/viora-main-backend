using Viora.Domain.Abstractions;

namespace Viora.Domain.Feedbacks.Events;

public sealed record FeedbackUpdatedEvent(Guid BranchId, int RatingOutOfTen) : IDomainEvent;
