using System.Globalization;
using System.Net;
using System.Text;
using Viora.Application.Analytics.Contracts;

namespace Viora.Api.Controllers.Analytics;

// Renders a DashboardData into a single self-contained HTML document: inline CSS, CSS-based bar charts (no
// JS, no external requests) so it renders even inside a sandboxed iframe. The frontend just embeds it.
internal static class DashboardHtmlRenderer
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static string Render(DashboardData d)
    {
        var sb = new StringBuilder(16_384);
        sb.Append("<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\">");
        sb.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
        sb.Append("<title>Organization Dashboard</title>");
        sb.Append("<style>").Append(Css).Append("</style></head><body>");

        // Header
        sb.Append("<header class=\"hd\"><h1>Organization Dashboard</h1><div class=\"sub\">")
          .Append(Enc($"{d.FromUtc:yyyy-MM-dd} → {d.ToUtc:yyyy-MM-dd}"))
          .Append(" &middot; ").Append(Enc(d.Granularity.ToString())).Append(" buckets &middot; ")
          .Append(Enc(d.Currency)).Append("</div></header>");

        // KPI cards
        sb.Append("<section class=\"cards\">");
        Card(sb, "Appointments", d.Appointments.Total.ToString("N0", Inv));
        Card(sb, "Completion rate", Pct(d.Appointments.CompletionRate));
        Card(sb, "No-show rate", Pct(d.Appointments.NoShowRate));
        Card(sb, "Cancellation rate", Pct(d.Appointments.CancellationRate));
        Card(sb, "Active customers", d.Customers.Active.ToString("N0", Inv));
        Card(sb, "Realized revenue", Money(d.Revenue.Realized, d.Currency));
        Card(sb, "Avg appointment", Money(d.Revenue.AverageAppointmentValue, d.Currency));
        Card(sb, "Potential revenue", Money(d.Revenue.Potential, d.Currency));
        sb.Append("</section>");

        // Time series
        sb.Append("<section class=\"grid2\">");
        VerticalBars(sb, "Appointments over time", d.AppointmentsOverTime, v => v.ToString("N0", Inv));
        VerticalBars(sb, "Revenue over time (completed)", d.RevenueOverTime, v => Money((decimal)v, d.Currency));
        sb.Append("</section>");

        // Breakdowns
        sb.Append("<section class=\"grid2\">");
        HorizontalBars(sb, "By status", d.ByStatus, c => c.Value.ToString("N0", Inv));
        HorizontalBars(sb, "By day of week", d.ByDayOfWeek, c => c.Value.ToString("N0", Inv));
        HorizontalBars(sb, "Top services", d.ByService, c => $"{c.Value:N0} &middot; {Money(c.Amount ?? 0m, d.Currency)}", rawValue: true);
        HorizontalBars(sb, "By branch", d.ByBranch, c => c.Value.ToString("N0", Inv));
        sb.Append("</section>");

        // Peak hours (full width)
        sb.Append("<section class=\"grid1\">");
        VerticalBars(sb, "Appointments by hour of day", ToPoints(d.ByHour), v => v.ToString("N0", Inv));
        sb.Append("</section>");

        // Customers + Billing
        sb.Append("<section class=\"grid2\">");
        sb.Append("<div class=\"panel\"><h2>Customers</h2><div class=\"mini\">");
        Mini(sb, "New", d.Customers.New.ToString("N0", Inv));
        Mini(sb, "Returning", d.Customers.Returning.ToString("N0", Inv));
        Mini(sb, "Active", d.Customers.Active.ToString("N0", Inv));
        sb.Append("</div></div>");

        sb.Append("<div class=\"panel\"><h2>Subscription billing</h2><div class=\"mini\">");
        Mini(sb, "Paid", Money(d.Billing.Paid, d.Currency));
        Mini(sb, "Outstanding", Money(d.Billing.Outstanding, d.Currency));
        Mini(sb, "Paid", d.Billing.PaidCount.ToString("N0", Inv));
        Mini(sb, "Issued", d.Billing.IssuedCount.ToString("N0", Inv));
        Mini(sb, "Overdue", d.Billing.OverdueCount.ToString("N0", Inv));
        Mini(sb, "Draft/Void", (d.Billing.DraftCount + d.Billing.VoidCount).ToString("N0", Inv));
        sb.Append("</div></div>");
        sb.Append("</section>");

        // Staff load table
        sb.Append("<section class=\"grid1\"><div class=\"panel\"><h2>Staff load</h2>");
        if (d.StaffLoad.Count == 0)
            sb.Append("<p class=\"empty\">No data in range.</p>");
        else
        {
            sb.Append("<table><thead><tr><th>Staff</th><th>Appointments</th><th>Booked hours</th></tr></thead><tbody>");
            foreach (var s in d.StaffLoad)
                sb.Append("<tr><td>").Append(Enc(s.Name)).Append("</td><td>")
                  .Append(s.Appointments.ToString("N0", Inv)).Append("</td><td>")
                  .Append(s.BookedHours.ToString("N1", Inv)).Append("</td></tr>");
            sb.Append("</tbody></table>");
        }
        sb.Append("</div></section>");

        sb.Append("</body></html>");
        return sb.ToString();
    }

    private static IReadOnlyList<TimePoint> ToPoints(IReadOnlyList<Category> categories) =>
        categories.Select(c => new TimePoint(c.Label, c.Value)).ToList();

    private static void Card(StringBuilder sb, string label, string value) =>
        sb.Append("<div class=\"card\"><div class=\"v\">").Append(Enc(value))
          .Append("</div><div class=\"l\">").Append(Enc(label)).Append("</div></div>");

    private static void Mini(StringBuilder sb, string label, string value) =>
        sb.Append("<div class=\"m\"><span class=\"mv\">").Append(Enc(value))
          .Append("</span><span class=\"ml\">").Append(Enc(label)).Append("</span></div>");

    // Vertical CSS bars for a time series. Scrolls horizontally when there are many buckets.
    private static void VerticalBars(StringBuilder sb, string title, IReadOnlyList<TimePoint> points, Func<double, string> fmt)
    {
        sb.Append("<div class=\"panel\"><h2>").Append(Enc(title)).Append("</h2>");
        if (points.Count == 0 || points.All(p => p.Value <= 0))
        {
            sb.Append("<p class=\"empty\">No data in range.</p></div>");
            return;
        }

        var max = points.Max(p => p.Value);
        var step = Math.Max(1, (int)Math.Ceiling(points.Count / 24.0)); // keep x labels readable

        sb.Append("<div class=\"vchart\">");
        for (var i = 0; i < points.Count; i++)
        {
            var p = points[i];
            var h = max <= 0 ? 0 : (int)Math.Round(p.Value / max * 160);
            var showLabel = i % step == 0;
            sb.Append("<div class=\"vcol\" title=\"").Append(Enc($"{p.Label}: {fmt(p.Value)}")).Append("\">")
              .Append("<div class=\"vbar\" style=\"height:").Append(h).Append("px\"></div>")
              .Append("<div class=\"vlbl\">").Append(showLabel ? Enc(p.Label) : "").Append("</div></div>");
        }
        sb.Append("</div></div>");
    }

    // Horizontal CSS bars for categories. rawValue=true means fmt returns pre-encoded HTML (already safe).
    private static void HorizontalBars(StringBuilder sb, string title, IReadOnlyList<Category> categories, Func<Category, string> fmt, bool rawValue = false)
    {
        sb.Append("<div class=\"panel\"><h2>").Append(Enc(title)).Append("</h2>");
        if (categories.Count == 0 || categories.All(c => c.Value <= 0))
        {
            sb.Append("<p class=\"empty\">No data in range.</p></div>");
            return;
        }

        var max = categories.Max(c => c.Value);
        sb.Append("<div class=\"hchart\">");
        foreach (var c in categories)
        {
            var w = max <= 0 ? 0 : (int)Math.Round(c.Value / max * 100);
            sb.Append("<div class=\"hrow\"><div class=\"hlbl\">").Append(Enc(c.Label)).Append("</div>")
              .Append("<div class=\"htrack\"><div class=\"hbar\" style=\"width:").Append(w).Append("%\"></div></div>")
              .Append("<div class=\"hval\">").Append(rawValue ? fmt(c) : Enc(fmt(c))).Append("</div></div>");
        }
        sb.Append("</div></div>");
    }

    private static string Pct(double rate) => rate.ToString("0.#", Inv) + "%";

    private static string Money(decimal amount, string currency) => $"{currency} {amount.ToString("N2", Inv)}";

    private static string Enc(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private const string Css = """
        :root{--bg:#f6f7fb;--panel:#fff;--ink:#201335;--muted:#6b7280;--accent:#201335;--bar:#7c5cff;--track:#eceafd;--line:#e5e7eb}
        *{box-sizing:border-box}
        body{margin:0;padding:24px;background:var(--bg);color:var(--ink);font:14px/1.4 system-ui,-apple-system,Segoe UI,Roboto,sans-serif}
        .hd h1{margin:0 0 4px;font-size:22px}
        .hd .sub{color:var(--muted);margin-bottom:20px}
        .cards{display:grid;grid-template-columns:repeat(auto-fit,minmax(150px,1fr));gap:12px;margin-bottom:16px}
        .card{background:var(--panel);border:1px solid var(--line);border-radius:12px;padding:14px 16px}
        .card .v{font-size:22px;font-weight:700}
        .card .l{color:var(--muted);font-size:12px;margin-top:2px}
        .grid1{display:grid;grid-template-columns:1fr;gap:16px;margin-bottom:16px}
        .grid2{display:grid;grid-template-columns:repeat(auto-fit,minmax(320px,1fr));gap:16px;margin-bottom:16px}
        .panel{background:var(--panel);border:1px solid var(--line);border-radius:12px;padding:16px;overflow:hidden}
        .panel h2{margin:0 0 14px;font-size:14px;color:var(--muted);text-transform:uppercase;letter-spacing:.04em}
        .empty{color:var(--muted);font-style:italic;margin:8px 0}
        .vchart{display:flex;align-items:flex-end;gap:6px;height:200px;overflow-x:auto;padding-bottom:4px}
        .vcol{flex:0 0 auto;width:26px;display:flex;flex-direction:column;align-items:center;justify-content:flex-end;height:100%}
        .vbar{width:18px;background:var(--bar);border-radius:4px 4px 0 0;min-height:2px}
        .vlbl{font-size:10px;color:var(--muted);margin-top:6px;white-space:nowrap;transform:rotate(-45deg);transform-origin:top left;height:24px}
        .hchart{display:flex;flex-direction:column;gap:8px}
        .hrow{display:grid;grid-template-columns:130px 1fr auto;align-items:center;gap:10px}
        .hlbl{font-size:12px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
        .htrack{background:var(--track);border-radius:6px;height:14px;overflow:hidden}
        .hbar{background:var(--bar);height:100%;border-radius:6px;min-width:2px}
        .hval{font-size:12px;color:var(--muted);white-space:nowrap}
        .mini{display:grid;grid-template-columns:repeat(auto-fit,minmax(90px,1fr));gap:10px}
        .m{display:flex;flex-direction:column;background:var(--bg);border-radius:10px;padding:10px}
        .mv{font-size:18px;font-weight:700}
        .ml{font-size:11px;color:var(--muted)}
        table{width:100%;border-collapse:collapse}
        th,td{text-align:left;padding:8px 10px;border-bottom:1px solid var(--line);font-size:13px}
        th{color:var(--muted);font-weight:600}
        """;
}
