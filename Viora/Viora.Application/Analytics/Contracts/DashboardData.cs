using Viora.Domain.Abstractions;

namespace Viora.Application.Analytics.Contracts;

public enum AnalyticsGranularity
{
    Day,
    Week,
    Month
}

// Fully-assembled, org-scoped dashboard data for the requested window. Rendered to HTML by the API layer.
public sealed record DashboardData(
    DateTime FromUtc,
    DateTime ToUtc,
    AnalyticsGranularity Granularity,
    string Currency,
    AppointmentSummary Appointments,
    RevenueSummary Revenue,
    IReadOnlyList<TimePoint> AppointmentsOverTime,
    IReadOnlyList<TimePoint> RevenueOverTime,
    IReadOnlyList<Category> ByStatus,
    IReadOnlyList<Category> ByService,
    IReadOnlyList<Category> ByBranch,
    IReadOnlyList<Category> ByDayOfWeek,
    IReadOnlyList<Category> ByHour,
    CustomerSummary Customers,
    IReadOnlyList<StaffLoad> StaffLoad,
    SubscriptionBilling Billing);

public sealed record AppointmentSummary(
    int Total,
    int Completed,
    int Canceled,
    int NoShow,
    int Pending,
    double CompletionRate,
    double NoShowRate,
    double CancellationRate);

public sealed record RevenueSummary(
    decimal Realized,
    decimal Potential,
    decimal AverageAppointmentValue);

// A labelled point in a time series (Value = count or amount depending on the series).
public sealed record TimePoint(string Label, double Value);

// A labelled category slice; Amount carried where a monetary total is also relevant (e.g. by service).
public sealed record Category(string Label, double Value, decimal? Amount = null);

public sealed record CustomerSummary(int Active, int New, int Returning);

public sealed record StaffLoad(string Name, int Appointments, double BookedHours);

// Organization's subscription/add-on invoices (Viora billing the org) within the window.
public sealed record SubscriptionBilling(
    decimal Paid,
    decimal Outstanding,
    int PaidCount,
    int IssuedCount,
    int OverdueCount,
    int VoidCount,
    int DraftCount);

public static class AnalyticsErrors
{
    public static readonly Error OrganizationMissing =
        new("Analytics.OrganizationMissing", "No organization is associated with the current user", ErrorCategory.Forbidden);

    public static readonly Error InvalidDateRange =
        new("Analytics.InvalidDateRange", "A valid 'from' and 'to' date range is required, with 'from' before 'to'", ErrorCategory.Validation);

    public static readonly Error InvalidGranularity =
        new("Analytics.InvalidGranularity", "Granularity must be one of: day, week, month", ErrorCategory.Validation);
}
