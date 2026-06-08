using Viora.Domain.Plans.Features;

namespace Viora.Infrastructure.Seeding.Data;

internal static class LimitedFeaturesData
{
    // wired from the data in the domain since it will be used from the domain as a reference from the commands as well
    public static IReadOnlyList<LimitedFeature> All => LimitedFeature.All;
}
