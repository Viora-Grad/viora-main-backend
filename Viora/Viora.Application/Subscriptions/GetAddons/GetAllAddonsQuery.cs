using Viora.Application.Abstractions.Messaging;
using Viora.Application.Plans.DTO;

namespace Viora.Application.Subscriptions.GetFeatureAddon;

public sealed record GetAllAddonsQuery() : IQuery<List<FeatureAddonDTO>>;

