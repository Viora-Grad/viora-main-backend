namespace Viora.Application.Marketing.GetQuota;

// Remaining = posts the org can still create this period. Provisioned=false means no usage row exists yet
// (org subscribed before the feature was added, or its plan does not grant it).
public sealed record MarketingQuotaResponse(
    long Remaining,
    bool Provisioned,
    DateTime? PeriodStart,
    DateTime? PeriodEnd);
