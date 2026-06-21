using Viora.Application.Abstractions.Messaging;
using Viora.Application.Plans.Shared;

namespace Viora.Application.Plans.GetLimitedFeature;

public record GetLimitedFeatureByIdQuery(Guid Id) : ICommand<FeatureResponse>;

