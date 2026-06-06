using Viora.Domain.Abstractions;

namespace Viora.Domain.Organizations.Suspensions.Events;

public record OrganizationDeletionDueEvent(Guid OrganizationId) : IDomainEvent;