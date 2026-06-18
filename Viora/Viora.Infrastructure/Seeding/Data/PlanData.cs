using Viora.Domain.Plans;
using Viora.Domain.Plans.Internal;

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
        99,
        PlanPeriod.monthly),

    Plan.Create(
        new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
        "Professional",
        "Professional Plan",
        "Advanced features for growing businesses",
        199,
        PlanPeriod.semiAnnually),

    Plan.Create(
        new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
        "Enterprise",
        "Enterprise Plan",
        "Full feature set for large organizations",
        399,
        PlanPeriod.annually)
    ];
}
