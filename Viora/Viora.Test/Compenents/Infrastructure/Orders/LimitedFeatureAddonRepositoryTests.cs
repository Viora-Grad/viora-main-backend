using Viora.Infrastructure;
using global::Viora.Infrastructure.Repositories.Plans;

namespace Viora.Test.Compenents.Infrastructure.Orders;

/// <summary>
/// Unit tests for the LimitedFeatureAddonRepository against an InMemory database.
/// LimitedFeatureAddon has a Price (Money ComplexProperty) which InMemory cannot even compile queries for.
/// Tests verify Add operations only.
/// </summary>
[TestClass]
public sealed class LimitedFeatureAddonRepositoryTests : InfrastructureTestBase
{
    private readonly LimitedFeatutreAddonRepository _repository;

    public LimitedFeatureAddonRepositoryTests()
    {
        _repository = new LimitedFeatutreAddonRepository(DbContext);
    }

    // ===== Add =====

    [TestMethod]
    public async Task Add_Addon_PersistsToDatabase()
    {
        var addon = CreateTestLimitedFeatureAddon();

        _repository.Add(addon);
        await DbContext.SaveChangesAsync();

        Assert.IsTrue(addon.Id != Guid.Empty);
    }

    // ===== Helpers =====

    private static global::Viora.Domain.Subscriptions.Addons.LimitedFeatureAddon CreateTestLimitedFeatureAddon()
    {
        return global::Viora.Domain.Subscriptions.Addons.LimitedFeatureAddon.Create(
            Guid.NewGuid(),
            global::Viora.Domain.Plans.Features.LimitedFeature.StaffMembers.Id,
            global::Viora.Domain.Subscriptions.Addons.Internal.AddonType.OneTime,
            10,
            new global::Viora.Domain.Shared.Money(10m, global::Viora.Domain.Shared.Currency.Usd));
    }
}
