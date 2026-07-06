using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Subscriptions.GetOrganizationUsage;

// Returns the current organization's plan-limited feature quota + usage. Org id comes from the token.
public sealed record GetOrganizationUsageQuery : IQuery<OrganizationUsageResponse>;
