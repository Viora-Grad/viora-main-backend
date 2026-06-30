using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Inventories.AddToInventory;

public sealed record AddToInventoryCommand(
    Guid BranchId,
    Guid UserId,
    Guid? PhotoId,
    string Name,
    string Notes,
    int Quantity,
    int MinimumThreshold) : ICommand<Guid>;