using Viora.Domain.Abstractions;

namespace Viora.Domain.Inventory.Events;

public sealed record MinimumThresholdReachedEvent(Guid ItemId, Guid BranchId, int Quantity) : IDomainEvent;
