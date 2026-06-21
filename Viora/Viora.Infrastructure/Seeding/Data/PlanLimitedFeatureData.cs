using Viora.Domain.Plans;

namespace Viora.Infrastructure.Seeding.Data;

internal class PlanLimitedFeatureData
{
    public static IReadOnlyList<PlanLimitedFeature> All { get; } =
    [
        PlanLimitedFeature.Create(
            new Guid("40000000-0000-0000-0000-000000000001"),
            PlanData.All[0].Id,
            LimitedFeaturesData.All[0].Id,
            1),

        PlanLimitedFeature.Create(
            new Guid("40000000-0000-0000-0000-000000000002"),
            PlanData.All[0].Id,
            LimitedFeaturesData.All[1].Id,
            5),

        PlanLimitedFeature.Create(
            new Guid("40000000-0000-0000-0000-000000000003"),
            PlanData.All[1].Id,
            LimitedFeaturesData.All[0].Id,
            5),

        PlanLimitedFeature.Create(
            new Guid("40000000-0000-0000-0000-000000000004"),
            PlanData.All[1].Id,
            LimitedFeaturesData.All[1].Id,
            25)
    ];

}
