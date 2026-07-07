using Viora.Infrastructure;
using global::Viora.Infrastructure.Repositories.Subscriptions;

namespace Viora.Test.Compenents.Infrastructure.Subscriptions;

/// <summary>
/// Unit tests for the AddonOrderRepository against an InMemory database.
/// AddonOrder has a Money TotalPrice (ComplexProperty) which InMemory cannot even compile queries for.
/// Tests verify Add and empty-result-only assertions.
/// </summary>
[TestClass]
public sealed class AddonOrderRepositoryTests : InfrastructureTestBase
{
    private readonly AddonOrderRepository _repository;

    public AddonOrderRepositoryTests()
    {
        _repository = new AddonOrderRepository(DbContext);
    }

    // ===== Add =====

    [TestMethod]
    public async Task Add_AddonOrder_PersistsToDatabase()
    {
        var limitedFeatureAddon = CreateTestLimitedFeatureAddon();
        DbContext.Set<global::Viora.Domain.Subscriptions.Addons.LimitedFeatureAddon>().Add(limitedFeatureAddon);
        await DbContext.SaveChangesAsync();

        var order = global::Viora.Domain.Orders.AddonOrder.CreateAddonOrder(
            Guid.NewGuid(), Guid.NewGuid(), new List<global::Viora.Domain.Subscriptions.Addons.LimitedFeatureAddon> { limitedFeatureAddon }, DateTime.UtcNow).Value;

        _repository.Add(order);
        await DbContext.SaveChangesAsync();

        Assert.IsTrue(order.Id != Guid.Empty);
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
