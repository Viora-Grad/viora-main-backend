namespace Viora.Application.Subscriptions.GetOrganizationUsage;

public sealed record OrganizationUsageResponse(
    Guid OrganizationId,
    IReadOnlyList<FeatureUsageResponse> Features);

// Per limited-feature quota for the current billing period.
//   Remaining = what's left (the stored FeatureUsage.Quota).
//   Limit     = the plan's granted allotment (null if the org has no active plan grant for it).
//   Used      = Limit - Remaining, best-effort (null when Limit is unknown; add-ons can raise Remaining above
//               the base Limit, so it's clamped at 0).
public sealed record FeatureUsageResponse(
    Guid LimitedFeatureId,
    string Key,
    string Description,
    long Remaining,
    long? Limit,
    long? Used,
    DateTime PeriodStart,
    DateTime PeriodEnd);
