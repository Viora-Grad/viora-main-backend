using Viora.Application.Analytics.Contracts;

namespace Viora.Application.Analytics.Abstractions;

// Read-side analytics engine (implemented in Infrastructure against the DbContext). Everything it returns is
// scoped to the given organization and the [fromUtc, toUtc] window.
public interface IOrganizationAnalyticsService
{
    Task<DashboardData> GetDashboardAsync(
        Guid organizationId,
        DateTime fromUtc,
        DateTime toUtc,
        AnalyticsGranularity granularity,
        CancellationToken cancellationToken);
}
