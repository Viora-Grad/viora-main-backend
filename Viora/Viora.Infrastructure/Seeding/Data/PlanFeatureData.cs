using Viora.Domain.Plans;

namespace Viora.Infrastructure.Seeding.Data;

internal class PlanFeatureData
{

    public static IReadOnlyList<PlanFeature> All { get; } =
    [
        PlanFeature.Create(
            new Guid("20000000-0000-0000-0000-000000000001"),
            PlanData.All[0].Id,
            FeatureData.All[0].Id),

        PlanFeature.Create(
            new Guid("20000000-0000-0000-0000-000000000002"),
            PlanData.All[0].Id,
            FeatureData.All[1].Id),

        PlanFeature.Create(
            new Guid("20000000-0000-0000-0000-000000000003"),
            PlanData.All[1].Id,
            FeatureData.All[0].Id),

        PlanFeature.Create(
            new Guid("20000000-0000-0000-0000-000000000004"),
            PlanData.All[1].Id,
            FeatureData.All[1].Id),

        PlanFeature.Create(
            new Guid("20000000-0000-0000-0000-000000000005"),
            PlanData.All[2].Id,
            FeatureData.All[0].Id)
    ];
}

