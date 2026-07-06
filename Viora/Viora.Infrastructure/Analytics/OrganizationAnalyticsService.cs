using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Viora.Application.Analytics.Abstractions;
using Viora.Application.Analytics.Contracts;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;
using Viora.Domain.Billings.Invoices;
using Viora.Domain.Billings.Invoices.Internals;
using Viora.Domain.Branches;
using Viora.Domain.Services;
using Viora.Domain.Staffs;

namespace Viora.Infrastructure.Analytics;

// Read-side analytics. Appointments are scoped to the org via Branch.OrganizationId; revenue is derived from
// Service.Cost (there is no cost on the appointment); invoices are the org's subscription bills. The filtered
// appointment set for one org + window is materialized once and aggregated in memory to keep the SQL simple.
internal sealed class OrganizationAnalyticsService(
    ApplicationDbContext db,
    IInvoiceRepository invoiceRepository) : IOrganizationAnalyticsService
{
    private static readonly CustomerStatus[] StatusOrder =
    [
        CustomerStatus.Completed, CustomerStatus.NotArrived, CustomerStatus.Waiting,
        CustomerStatus.InProgress, CustomerStatus.NoShow, CustomerStatus.Canceled
    ];

    private static readonly DayOfWeek[] WeekOrder =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
        DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
    ];

    private sealed record Row(DateTime ReservationDate, CustomerStatus Status, Guid ServiceId, Guid StaffId, Guid BranchId, Guid? CustomerId, int DurationMinutes);

    public async Task<DashboardData> GetDashboardAsync(
        Guid organizationId, DateTime fromUtc, DateTime toUtc, AnalyticsGranularity granularity, CancellationToken cancellationToken)
    {
        // --- Reference data (labels + service costs), scoped to the org ---
        var branches = await db.Set<Branch>()
            .Where(b => b.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);
        var branchIds = branches.Select(b => b.Id).ToList();
        var branchLabel = branches.ToDictionary(b => b.Id, b => b.Address.Value);

        var services = await db.Set<Service>()
            .Where(s => branchIds.Contains(s.BranchId))
            .ToListAsync(cancellationToken);
        var serviceName = services.ToDictionary(s => s.Id, s => s.Name.Value);
        var serviceCost = services.ToDictionary(s => s.Id, s => s.Cost.Amount);
        var currency = services.Select(s => s.Cost.Currency.Code).FirstOrDefault() ?? "USD";

        var staff = await db.Set<Staff>()
            .Where(s => s.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);
        var staffLabel = staff.ToDictionary(
            s => s.Id,
            s => $"{s.FirstName?.Value} {s.LastName?.Value}".Trim() is { Length: > 0 } n ? n : "Unnamed staff");

        // --- Appointments in window (org-scoped via branch) ---
        var rows = await db.Set<Appointment>()
            .Where(a => branchIds.Contains(a.BranchId) && a.ReservationDate >= fromUtc && a.ReservationDate <= toUtc)
            .Select(a => new Row(a.ReservationDate, a.Status, a.ServiceId, a.StaffId, a.BranchId, a.CustomerId, a.EstimatedDurationMinutes))
            .ToListAsync(cancellationToken);

        // First-ever appointment per customer (all time, org) — for new-vs-returning classification.
        var firstSeenList = await db.Set<Appointment>()
            .Where(a => branchIds.Contains(a.BranchId) && a.CustomerId != null)
            .GroupBy(a => a.CustomerId!.Value)
            .Select(g => new { CustomerId = g.Key, First = g.Min(x => x.ReservationDate) })
            .ToListAsync(cancellationToken);
        var firstSeen = firstSeenList.ToDictionary(x => x.CustomerId, x => x.First);

        // --- Invoices (subscription billing) in window ---
        var invoices = await invoiceRepository.GetAllByOrganizationIdAsync(organizationId, cancellationToken);
        var windowInvoices = invoices.Where(i => i.CreatedAtUtc >= fromUtc && i.CreatedAtUtc <= toUtc).ToList();

        // --- Aggregate ---
        var total = rows.Count;
        var completed = rows.Count(r => r.Status == CustomerStatus.Completed);
        var canceled = rows.Count(r => r.Status == CustomerStatus.Canceled);
        var noShow = rows.Count(r => r.Status == CustomerStatus.NoShow);
        var pending = total - completed - canceled - noShow;

        double Rate(int n) => total == 0 ? 0 : Math.Round(100.0 * n / total, 1);

        var appointmentSummary = new AppointmentSummary(
            total, completed, canceled, noShow, pending, Rate(completed), Rate(noShow), Rate(canceled));

        decimal CostOf(Guid serviceId) => serviceCost.GetValueOrDefault(serviceId, 0m);
        var realized = rows.Where(r => r.Status == CustomerStatus.Completed).Sum(r => CostOf(r.ServiceId));
        var potential = rows.Sum(r => CostOf(r.ServiceId));
        var revenueSummary = new RevenueSummary(
            realized, potential, completed == 0 ? 0m : Math.Round(realized / completed, 2));

        var buckets = EnumerateBuckets(fromUtc, toUtc, granularity).ToList();

        var apptByBucket = rows
            .GroupBy(r => BucketStart(r.ReservationDate, granularity))
            .ToDictionary(g => g.Key, g => g.Count());
        var appointmentsOverTime = buckets
            .Select(b => new TimePoint(BucketLabel(b, granularity), apptByBucket.GetValueOrDefault(b, 0)))
            .ToList();

        var revByBucket = rows
            .Where(r => r.Status == CustomerStatus.Completed)
            .GroupBy(r => BucketStart(r.ReservationDate, granularity))
            .ToDictionary(g => g.Key, g => (double)g.Sum(r => CostOf(r.ServiceId)));
        var revenueOverTime = buckets
            .Select(b => new TimePoint(BucketLabel(b, granularity), revByBucket.GetValueOrDefault(b, 0d)))
            .ToList();

        var byStatus = StatusOrder
            .Select(s => new Category(s.ToString(), rows.Count(r => r.Status == s)))
            .ToList();

        var byService = rows
            .GroupBy(r => r.ServiceId)
            .Select(g => new Category(serviceName.GetValueOrDefault(g.Key, "Unknown service"), g.Count(), g.Sum(r => CostOf(r.ServiceId))))
            .OrderByDescending(c => c.Value)
            .Take(10)
            .ToList();

        var byBranch = rows
            .GroupBy(r => r.BranchId)
            .Select(g => new Category(branchLabel.GetValueOrDefault(g.Key, "Unknown branch"), g.Count()))
            .OrderByDescending(c => c.Value)
            .ToList();

        var byDayOfWeek = WeekOrder
            .Select(d => new Category(d.ToString(), rows.Count(r => r.ReservationDate.DayOfWeek == d)))
            .ToList();

        var byHour = Enumerable.Range(0, 24)
            .Select(h => new Category($"{h:00}:00", rows.Count(r => r.ReservationDate.Hour == h)))
            .ToList();

        var customerIds = rows.Where(r => r.CustomerId is not null).Select(r => r.CustomerId!.Value).Distinct().ToList();
        var newCustomers = customerIds.Count(id => firstSeen.TryGetValue(id, out var first) && first >= fromUtc);
        var customerSummary = new CustomerSummary(customerIds.Count, newCustomers, customerIds.Count - newCustomers);

        var staffLoad = rows
            .GroupBy(r => r.StaffId)
            .Select(g => new StaffLoad(
                staffLabel.GetValueOrDefault(g.Key, "Unknown staff"),
                g.Count(),
                Math.Round(g.Sum(r => r.DurationMinutes) / 60.0, 1)))
            .OrderByDescending(s => s.Appointments)
            .Take(10)
            .ToList();

        decimal AmountOf(InvoiceStatus st) => windowInvoices.Where(i => i.Status == st).Sum(i => i.Total.Amount);
        int CountOf(InvoiceStatus st) => windowInvoices.Count(i => i.Status == st);
        var billing = new SubscriptionBilling(
            AmountOf(InvoiceStatus.Paid),
            AmountOf(InvoiceStatus.Issued) + AmountOf(InvoiceStatus.Overdue),
            CountOf(InvoiceStatus.Paid), CountOf(InvoiceStatus.Issued), CountOf(InvoiceStatus.Overdue),
            CountOf(InvoiceStatus.Void), CountOf(InvoiceStatus.Draft));

        return new DashboardData(
            fromUtc, toUtc, granularity, currency,
            appointmentSummary, revenueSummary,
            appointmentsOverTime, revenueOverTime,
            byStatus, byService, byBranch, byDayOfWeek, byHour,
            customerSummary, staffLoad, billing);
    }

    private static DateTime BucketStart(DateTime d, AnalyticsGranularity g) => g switch
    {
        AnalyticsGranularity.Week => d.Date.AddDays(-(int)d.Date.DayOfWeek), // Sunday-start week
        AnalyticsGranularity.Month => new DateTime(d.Year, d.Month, 1),
        _ => d.Date,
    };

    private static IEnumerable<DateTime> EnumerateBuckets(DateTime from, DateTime to, AnalyticsGranularity g)
    {
        var current = BucketStart(from, g);
        var end = BucketStart(to, g);
        while (current <= end)
        {
            yield return current;
            current = g switch
            {
                AnalyticsGranularity.Week => current.AddDays(7),
                AnalyticsGranularity.Month => current.AddMonths(1),
                _ => current.AddDays(1),
            };
        }
    }

    private static string BucketLabel(DateTime b, AnalyticsGranularity g) => g == AnalyticsGranularity.Month
        ? b.ToString("MMM yyyy", CultureInfo.InvariantCulture)
        : b.ToString("MMM d", CultureInfo.InvariantCulture);
}
