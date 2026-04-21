using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Plans.PurchaseAddon;

public sealed record PurchaseAddonCommand(Guid SubscriptionId,
    Guid LimitedFeatureId,
    Guid LimitedFeatureAddonId) : ICommand;