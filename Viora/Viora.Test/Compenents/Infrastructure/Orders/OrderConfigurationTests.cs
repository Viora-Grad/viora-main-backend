using Viora.Infrastructure;

namespace Viora.Test.Compenents.Infrastructure.Orders;

/// <summary>
/// Unit tests for Order entity configurations against an InMemory database.
/// Entities with Money (ComplexProperty) cannot be queried via InMemory. Tests verify Add and domain behavior only.
/// </summary>
[TestClass]
public sealed class OrderConfigurationTests : InfrastructureTestBase
{
    // ===== SubscriptionOrder =====

    [TestMethod]
    public async Task SubscriptionOrder_CreateAndPersist_Succeeds()
    {
        var plan = CreateTestPlan();
        var order = global::Viora.Domain.Orders.SubscriptionOrder.CreateNewSubscriptionOrder(
            Guid.NewGuid(), plan, DateTime.UtcNow).Value;

        DbContext.Set<global::Viora.Domain.Plans.Plan>().Add(plan);
        DbContext.Set<global::Viora.Domain.Orders.SubscriptionOrder>().Add(order);
        await DbContext.SaveChangesAsync();

        Assert.IsTrue(order.Id != Guid.Empty);
    }

    [TestMethod]
    public async Task SubscriptionOrder_HasCorrectOrganizationId()
    {
        var orgId = Guid.NewGuid();
        var plan = CreateTestPlan();
        var order = global::Viora.Domain.Orders.SubscriptionOrder.CreateNewSubscriptionOrder(
            orgId, plan, DateTime.UtcNow).Value;

        Assert.AreEqual(orgId, order.OrganizationId);
    }

    [TestMethod]
    public async Task SubscriptionOrder_MarkPending_SetsStatusCorrectly()
    {
        var plan = CreateTestPlan();
        var order = global::Viora.Domain.Orders.SubscriptionOrder.CreateNewSubscriptionOrder(
            Guid.NewGuid(), plan, DateTime.UtcNow).Value;

        order.MarkPending();

        Assert.AreEqual(global::Viora.Domain.Orders.Internal.OrderStatus.Pending, order.Status);
    }

    // ===== AddonOrder =====

    [TestMethod]
    public async Task AddonOrder_CreateAndPersist_Succeeds()
    {
        var addon = CreateTestLimitedFeatureAddon();
        DbContext.Set<global::Viora.Domain.Subscriptions.Addons.LimitedFeatureAddon>().Add(addon);
        await DbContext.SaveChangesAsync();

        var order = global::Viora.Domain.Orders.AddonOrder.CreateAddonOrder(
            Guid.NewGuid(), Guid.NewGuid(), new List<global::Viora.Domain.Subscriptions.Addons.LimitedFeatureAddon> { addon }, DateTime.UtcNow).Value;

        DbContext.Set<global::Viora.Domain.Orders.AddonOrder>().Add(order);
        await DbContext.SaveChangesAsync();

        Assert.IsTrue(order.Id != Guid.Empty);
    }

    [TestMethod]
    public async Task AddonOrder_WithMultipleAddons_PersistsCorrectly()
    {
        var addon1 = CreateTestLimitedFeatureAddon();
        var addon2 = CreateTestLimitedFeatureAddon();
        DbContext.Set<global::Viora.Domain.Subscriptions.Addons.LimitedFeatureAddon>().AddRange(addon1, addon2);
        await DbContext.SaveChangesAsync();

        var order = global::Viora.Domain.Orders.AddonOrder.CreateAddonOrder(
            Guid.NewGuid(), Guid.NewGuid(), new List<global::Viora.Domain.Subscriptions.Addons.LimitedFeatureAddon> { addon1, addon2 }, DateTime.UtcNow).Value;

        DbContext.Set<global::Viora.Domain.Orders.AddonOrder>().Add(order);
        await DbContext.SaveChangesAsync();

        Assert.IsTrue(order.Id != Guid.Empty);
    }

    // ===== Helpers =====

    private static global::Viora.Domain.Plans.Plan CreateTestPlan()
    {
        return global::Viora.Domain.Plans.Plan.Create(Guid.NewGuid(), "Test Plan", "Description", "Content", 99.99m, global::Viora.Domain.Shared.Currency.Usd, global::Viora.Domain.Plans.Internal.PlanPeriod.monthly);
    }

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
