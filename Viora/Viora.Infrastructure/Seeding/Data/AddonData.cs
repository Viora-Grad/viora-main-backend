using Viora.Domain.Subscriptions.Addons;
using Viora.Domain.Subscriptions.Addons.Internal;

namespace Viora.Infrastructure.Seeding.Data;

internal class AddonData
{
    public static IReadOnlyList<LimitedFeatureAddon> All { get; } =
    [
        LimitedFeatureAddon.Create(
            new Guid("11111111-1111-1111-1111-111111111111"),
            new Guid("F1A2B3C4-0001-0000-0000-000000000001"),
            AddonType.OneTime,
            10,
            29.99
        ),

        LimitedFeatureAddon.Create(
            new Guid("22222222-2222-2222-2222-222222222222"),
            new Guid("F1A2B3C4-0002-0000-0000-000000000002"),
            AddonType.OneTime,
            25,
            59.99
        ),

        LimitedFeatureAddon.Create(
            new Guid("33333333-3333-3333-3333-333333333333"),
            new Guid("F1A2B3C4-0003-0000-0000-000000000003"),
            AddonType.TimeBase,
            50,
            99.99
        ),

        LimitedFeatureAddon.Create(
            new Guid("44444444-4444-4444-4444-444444444444"),
            new Guid("F1A2B3C4-0004-0000-0000-000000000004"),
            AddonType.TimeBase,
            100,
            149.99
        )
    ];
}
