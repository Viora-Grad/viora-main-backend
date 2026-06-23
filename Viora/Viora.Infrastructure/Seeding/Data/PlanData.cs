using Viora.Domain.Plans;
using Viora.Domain.Plans.Internal;
using Viora.Domain.Shared;

namespace Viora.Infrastructure.Seeding.Data;

internal static class PlanData
{
    public static IReadOnlyList<Plan> All { get; } =
    [
    Plan.Create(
        new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
        "Starter",
        "Starter Plan",
        "Basic features for small businesses",
        (decimal)99.9,
        Currency.Egp,
        PlanPeriod.monthly),

    Plan.Create(
        new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
        "Professional",
        "Professional Plan",
        "Advanced features for growing businesses",
        (decimal)199.9,
        Currency.Egp,
        PlanPeriod.semiAnnually),

    Plan.Create(
        new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
        "Enterprise",
        "Enterprise Plan",
        "Full feature set for large organizations",
        (decimal)399.9,
        Currency.Egp,
        PlanPeriod.annually),

    ];
}
