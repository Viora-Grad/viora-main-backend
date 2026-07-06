using Viora.Domain.Plans;

namespace Viora.Infrastructure.Seeding.Data;

internal class PlanFeatureData
{

    public static IReadOnlyList<PlanFeature> All { get; } =
    [
        //first plan
        PlanFeature.Create(
            new Guid("20000000-0000-0000-0000-000000000001"),
            PlanData.All[0].Id,
            FeatureData.All[0].Id),

        PlanFeature.Create(
            new Guid("20000000-0000-0000-0000-000000000002"),
            PlanData.All[0].Id,
            FeatureData.All[1].Id),
        PlanFeature.Create(
            new Guid("20000000-0000-0000-0000-000000000006"),
            PlanData.All[0].Id,
            FeatureData.All[2].Id),
        PlanFeature.Create(
            new Guid("20000000-0000-0000-0000-000000000007"),
            PlanData.All[0].Id,
            FeatureData.All[3].Id),
        PlanFeature.Create(
            new Guid("20000000-0000-0000-0000-000000000008"),
            PlanData.All[0].Id,
            FeatureData.All[4].Id),

        // second plan
        PlanFeature.Create(
            new Guid("20000000-0000-0000-0000-000000000003"),
            PlanData.All[1].Id,
            FeatureData.All[0].Id),

        PlanFeature.Create(
            new Guid("20000000-0000-0000-0000-000000000004"),
            PlanData.All[1].Id,
            FeatureData.All[1].Id),
        PlanFeature.Create(
            new Guid("20000000-0000-0000-0000-000000000009"),
            PlanData.All[1].Id,
            FeatureData.All[2].Id),

        PlanFeature.Create(
            new Guid ("20000000-0000-0000-0000-000000000010"),
            PlanData.All[1].Id,
            FeatureData.All[3].Id),

        PlanFeature.Create(
            new Guid("20000000-0000-0000-0000-000000000011"),
            PlanData.All[1].Id,
            FeatureData.All[4].Id),

        // third plan
        PlanFeature.Create(
            new Guid("20000000-0000-0000-0000-000000000005"),
            PlanData.All[2].Id,
            FeatureData.All[0].Id),

        PlanFeature.Create(
            new Guid("20000000-0000-0000-0000-000000000012"),
            PlanData.All[2].Id,
            FeatureData.All[1].Id),
        PlanFeature.Create(
            new Guid("20000000-0000-0000-0000-000000000013"),
            PlanData.All[2].Id,
            FeatureData.All[2].Id),
        PlanFeature.Create(
            new Guid("20000000-0000-0000-0000-000000000014"),
            PlanData.All[2].Id,
            FeatureData.All[3].Id),
        PlanFeature.Create(
            new Guid("20000000-0000-0000-0000-000000000015"),
            PlanData.All[2].Id,
            FeatureData.All[4].Id)
    ];
}

