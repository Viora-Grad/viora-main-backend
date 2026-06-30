using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Inventories.UpdateInventoryItem;

public sealed record UpdateInventoryItemCommand(
    Guid ItemId,
    Guid? PhotoId,
    string Name,
    string Notes,
    int MinimumThreshold) : ICommand;