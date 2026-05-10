using Viora.Application.Abstractions.Messaging;
using Viora.Application.Subscriptions.GetAddons;

namespace Viora.Application.Subscriptions.GetFeatureAddon;

public sealed record GetAllAddonsQuery() : IQuery<List<FeatureAddonResponse>>;

