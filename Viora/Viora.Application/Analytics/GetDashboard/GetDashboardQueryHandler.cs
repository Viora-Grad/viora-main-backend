using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Analytics.Abstractions;
using Viora.Application.Analytics.Contracts;
using Viora.Domain.Abstractions;

namespace Viora.Application.Analytics.GetDashboard;

internal sealed class GetDashboardQueryHandler(
    IOrganizationAnalyticsService analyticsService,
    IUserContext userContext) : IQueryHandler<GetDashboardQuery, DashboardData>
{
    public async Task<Result<DashboardData>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        if (userContext.OrganizationId is not { } organizationId)
            return Result.Failure<DashboardData>(AnalyticsErrors.OrganizationMissing);

        if (request.From is not { } from || request.To is not { } to || from >= to)
            return Result.Failure<DashboardData>(AnalyticsErrors.InvalidDateRange);

        if (!TryParseGranularity(request.Granularity, out var granularity))
            return Result.Failure<DashboardData>(AnalyticsErrors.InvalidGranularity);

        // Treat the incoming dates as UTC instants (the store keeps everything in UTC).
        var fromUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        var toUtc = DateTime.SpecifyKind(to, DateTimeKind.Utc);

        var data = await analyticsService.GetDashboardAsync(organizationId, fromUtc, toUtc, granularity, cancellationToken);
        return Result.Success(data);
    }

    private static bool TryParseGranularity(string? value, out AnalyticsGranularity granularity)
    {
        // Default to Day when omitted; otherwise it must be a valid value.
        if (string.IsNullOrWhiteSpace(value))
        {
            granularity = AnalyticsGranularity.Day;
            return true;
        }

        return Enum.TryParse(value, ignoreCase: true, out granularity)
            && Enum.IsDefined(granularity);
    }
}
