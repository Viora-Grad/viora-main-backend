using Viora.Application.Abstractions.Messaging;
using Viora.Application.Plans.DTO;

namespace Viora.Application.Plans.GetFeatureAddon;

public sealed record GetFeatureAddonQuery(Guid LimitedFeatureId) : IQuery<List<FeatureAddonDTO>>;

