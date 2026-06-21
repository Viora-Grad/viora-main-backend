using Viora.Domain.Plans.Features;

namespace Viora.Infrastructure.Seeding.Data;

internal class FeatureData
{
    // TODO make this wired like LimitedFeatureData.cs
    public static IReadOnlyList<Feature> All { get; } =
        [
             Feature.Create(
            new Guid("10000000-0000-0000-0000-000000000001"),
            "Scheduling",
            "Manage appointments and schedules"),

            Feature.Create(
                new Guid("10000000-0000-0000-0000-000000000002"),
                "Inventory",
                "Manage products and stock"),

            Feature.Create(
                new Guid("10000000-0000-0000-0000-000000000003"),
                "Chat",
                "Communicate with customers"),

            Feature.Create(
                new Guid("10000000-0000-0000-0000-000000000004"),
                "Archive",
                "Archive and restore data"),

            Feature.Create(
                new Guid("10000000-0000-0000-0000-000000000005"),
                "StaffManagement",
                "Manage staff members")
    ];
}
