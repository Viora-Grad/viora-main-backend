using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Inventories.GetInventoryItemImage;

public sealed record GetInventoryItemImageQuery(Guid ItemId) : IQuery<MediaResponseStream>;
