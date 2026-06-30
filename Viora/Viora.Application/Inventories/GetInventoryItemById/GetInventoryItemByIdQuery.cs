using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Inventories.GetInventoryItemById;

public sealed record GetInventoryItemByIdQuery(Guid ItemId) : IQuery<InventoryItemDetailsResponse>;
