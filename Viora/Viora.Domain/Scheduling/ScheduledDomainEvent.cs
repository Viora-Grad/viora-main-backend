using Viora.Domain.Abstractions;

namespace Viora.Domain.Scheduling;

/// <summary>
/// represents a domain event that is to be executed on a scheduled time, storage and tracking is done in the DB while represented as entity
/// </summary>
public class ScheduledDomainEvent : Entity
{
    public string EventType { get; private set; } = default!;       // e.g. "OrganizationDeletionDue"
    public string Payload { get; private set; } = default!;         // serialized JSON
    public DateTimeOffset ScheduledFor { get; private set; }
    public DateTimeOffset? ProcessedOn { get; private set; }
    public bool IsCancelled { get; private set; } = false;
    public int AttemptCount { get; private set; }
    public string? Error { get; private set; }

    private ScheduledDomainEvent() { } // EF

    public static Result<ScheduledDomainEvent> Create(string eventType, string payload, DateTimeOffset scheduledFor)
    {
        return Result.Success<ScheduledDomainEvent>(new()
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            Payload = payload,
            ScheduledFor = scheduledFor,
            AttemptCount = 0
        });
    }

    public Result MarkProcessed(DateTimeOffset referenceTime)
    {
        ProcessedOn = referenceTime;
        return Result.Success();
    }

    public Result RecordFailure(string error)
    {
        AttemptCount++;
        Error = error;
        return Result.Success();
    }

    public Result Cancel(DateTimeOffset referenceTime)
    {
        IsCancelled = true;
        ProcessedOn = referenceTime; // soft-cancel
        return Result.Success();
    }
}
