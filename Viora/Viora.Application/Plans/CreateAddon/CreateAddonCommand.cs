using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Plans.CreateAddon;

public sealed record CreateAddonCommand(Guid SubscriptionId,
    Guid LimitedFeatureAddon,
    Guid LimitedFeatureId) : ICommand;
