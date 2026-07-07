using Viora.Domain.Plans;

namespace Viora.Infrastructure.Seeding.Data;

internal class PlanLimitedFeatureData
{
    public static IReadOnlyList<PlanLimitedFeature> All { get; } =
    [
        // first plan
        PlanLimitedFeature.Create(
            new Guid("40000000-0000-0000-0000-000000000001"),
            PlanData.All[0].Id,
            LimitedFeaturesData.All[0].Id,
            3),

        PlanLimitedFeature.Create(
            new Guid("40000000-0000-0000-0000-000000000002"),
            PlanData.All[0].Id,
            LimitedFeaturesData.All[1].Id,
            25),

         PlanLimitedFeature.Create(
            new Guid("40000000-0000-0000-0000-000000000005"),
            PlanData.All[0].Id,
            LimitedFeaturesData.All[2].Id,
            10),

          PlanLimitedFeature.Create(
            new Guid("40000000-0000-0000-0000-000000000008"),
            PlanData.All[0].Id,
            LimitedFeaturesData.All[3].Id,
            50000),

            PlanLimitedFeature.Create(
            new Guid("40000000-0000-0000-0000-000000000007"),
            PlanData.All[0].Id,
            LimitedFeaturesData.All[4].Id,
            5),


        //second plan
        PlanLimitedFeature.Create(
            new Guid("40000000-0000-0000-0000-000000000003"),
            PlanData.All[1].Id,
            LimitedFeaturesData.All[0].Id,
            5),

        PlanLimitedFeature.Create(
            new Guid("40000000-0000-0000-0000-000000000004"),
            PlanData.All[1].Id,
            LimitedFeaturesData.All[1].Id,
            35),
        PlanLimitedFeature.Create(
            new Guid("40000000-0000-0000-0000-000000000009"),
            PlanData.All[1].Id,
            LimitedFeaturesData.All[2].Id,
            20),

        PlanLimitedFeature.Create(
            new Guid("40000000-0000-0000-0000-000000000010"),
            PlanData.All[1].Id,
            LimitedFeaturesData.All[3].Id,
            100000),

        // Marketing AI posts (LimitedFeaturesData.All[4]) for the second plan.
        PlanLimitedFeature.Create(
            new Guid("40000000-0000-0000-0000-000000000016"),
            PlanData.All[1].Id,
            LimitedFeaturesData.All[4].Id,
            5),




        // third plan 

        PlanLimitedFeature.Create(
            new Guid("40000000-0000-0000-0000-000000000011"),
            PlanData.All[2].Id,
            LimitedFeaturesData.All[0].Id,
            10),

        PlanLimitedFeature.Create(
            new Guid("40000000-0000-0000-0000-000000000012"),
            PlanData.All[2].Id,
            LimitedFeaturesData.All[1].Id,
            50),

        PlanLimitedFeature.Create(
            new Guid("40000000-0000-0000-0000-000000000013"),
            PlanData.All[2].Id,
            LimitedFeaturesData.All[2].Id,
            40),

        PlanLimitedFeature.Create(
            new Guid("40000000-0000-0000-0000-000000000014"),
            PlanData.All[2].Id,
            LimitedFeaturesData.All[3].Id,
            200000),

        PlanLimitedFeature.Create(
            new Guid("40000000-0000-0000-0000-000000000015"),
            PlanData.All[2].Id,
            LimitedFeaturesData.All[4].Id,
            10)
    ];

}
