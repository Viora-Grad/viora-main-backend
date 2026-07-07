using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Viora.Domain.Orders;
using Viora.Domain.Orders.Internal;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;
using Viora.Domain.Plans.Internal;
using Viora.Domain.Shared;
using Viora.Domain.Subscriptions;
using Viora.Domain.Subscriptions.Addons;
using Viora.Domain.Subscriptions.Addons.Internal;
using Viora.Domain.Subscriptions.Internal;
using Viora.Infrastructure;

namespace Viora.Test.Integerations;

[TestClass]
public sealed class OrdersIntegrationTests
{
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly ApplicationDbContext _dbContext;
    private static readonly Guid OrgId = Guid.NewGuid();
    private static readonly DateTime FixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

    public OrdersIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options, _publisherMock.Object);
    }

    [TestCleanup]
    public void Cleanup() => _dbContext.Dispose();

    private async Task<Plan> SeedPlanAsync(string name, decimal price)
    {
        var plan = Plan.Create(Guid.NewGuid(), name, $"{name} tier", "desc", price, Currency.Usd, PlanPeriod.monthly);
        _dbContext.Add(plan);
        await _dbContext.SaveChangesAsync();
        return plan;
    }

    [TestMethod]
    public async Task CreateNewSubscriptionOrder_WithPlan_PersistsDraftOrder()
    {
        var plan = await SeedPlanAsync("Pro Plan", 49.99m);

        var orderResult = SubscriptionOrder.CreateNewSubscriptionOrder(OrgId, plan, FixedNow);
        Assert.IsTrue(orderResult.IsSuccess);

        var order = orderResult.Value;
        _dbContext.Add(order);
        await _dbContext.SaveChangesAsync();

        var tracked = _dbContext.ChangeTracker.Entries<SubscriptionOrder>()
            .FirstOrDefault(e => e.Entity.Id == order.Id);
        Assert.IsNotNull(tracked);
        Assert.AreEqual(OrgId, tracked.Entity.OrganizationId);
        Assert.AreEqual(plan.Id, tracked.Entity.PlanId);
        Assert.AreEqual(OrderStatus.Draft, tracked.Entity.Status);
    }

    [TestMethod]
    public async Task CreateRenewSubscriptionOrder_WithExistingSubscription_PersistsRenewalOrder()
    {
        var plan = await SeedPlanAsync("Basic Plan", 19.99m);

        var subscriptionResult = Subscription.Create(plan.Id, OrgId, FixedNow, FixedNow.AddMonths(1));
        Assert.IsTrue(subscriptionResult.IsSuccess);
        var subscription = subscriptionResult.Value;
        _dbContext.Add(subscription);
        await _dbContext.SaveChangesAsync();

        var orderResult = SubscriptionOrder.CreateRenewSubscriptionOrder(
            OrgId, plan.Id, subscription.Id, plan.Price, FixedNow);
        Assert.IsTrue(orderResult.IsSuccess);

        var order = orderResult.Value;
        _dbContext.Add(order);
        await _dbContext.SaveChangesAsync();

        var tracked = _dbContext.ChangeTracker.Entries<SubscriptionOrder>()
            .FirstOrDefault(e => e.Entity.Id == order.Id);
        Assert.IsNotNull(tracked);
        Assert.AreEqual(subscription.Id, tracked.Entity.SubscriptionId);
        Assert.AreEqual(OrderStatus.Draft, tracked.Entity.Status);
    }

    [TestMethod]
    public async Task CreateChangeSubscriptionOrder_WithNewPlan_PersistsChangeOrder()
    {
        var oldPlan = await SeedPlanAsync("Basic Plan", 19.99m);
        var newPlan = await SeedPlanAsync("Premium Plan", 79.99m);

        var subscriptionResult = Subscription.Create(oldPlan.Id, OrgId, FixedNow, FixedNow.AddMonths(1));
        Assert.IsTrue(subscriptionResult.IsSuccess);
        var subscription = subscriptionResult.Value;
        _dbContext.Add(subscription);
        await _dbContext.SaveChangesAsync();

        var orderResult = SubscriptionOrder.CreateChangeSubscriptionOrder(OrgId, subscription.Id, newPlan, FixedNow);
        Assert.IsTrue(orderResult.IsSuccess);

        var order = orderResult.Value;
        _dbContext.Add(order);
        await _dbContext.SaveChangesAsync();

        var tracked = _dbContext.ChangeTracker.Entries<SubscriptionOrder>()
            .FirstOrDefault(e => e.Entity.Id == order.Id);
        Assert.IsNotNull(tracked);
        Assert.AreEqual(newPlan.Id, tracked.Entity.PlanId);
        Assert.AreEqual(subscription.Id, tracked.Entity.SubscriptionId);
        Assert.AreEqual(OrderStatus.Draft, tracked.Entity.Status);
    }

    [TestMethod]
    public async Task CreateAddonOrder_WithMultipleAddons_PersistsDraftOrder()
    {
        var addon1 = LimitedFeatureAddon.Create(Guid.NewGuid(), LimitedFeature.Branches.Id, AddonType.OneTime, 5, new Money(10m, Currency.Usd));
        var addon2 = LimitedFeatureAddon.Create(Guid.NewGuid(), LimitedFeature.StaffMembers.Id, AddonType.TimeBase, 3, new Money(15m, Currency.Usd));
        _dbContext.AddRange(addon1, addon2);
        await _dbContext.SaveChangesAsync();

        var subscriptionResult = Subscription.Create(Guid.NewGuid(), OrgId, FixedNow, FixedNow.AddMonths(1));
        Assert.IsTrue(subscriptionResult.IsSuccess);
        var subscription = subscriptionResult.Value;
        _dbContext.Add(subscription);
        await _dbContext.SaveChangesAsync();

        var orderResult = AddonOrder.CreateAddonOrder(OrgId, subscription.Id, new List<LimitedFeatureAddon> { addon1, addon2 }, FixedNow);
        Assert.IsTrue(orderResult.IsSuccess);

        var order = orderResult.Value;
        _dbContext.Add(order);
        await _dbContext.SaveChangesAsync();

        var tracked = _dbContext.ChangeTracker.Entries<AddonOrder>()
            .FirstOrDefault(e => e.Entity.Id == order.Id);
        Assert.IsNotNull(tracked);
        Assert.AreEqual(OrgId, tracked.Entity.OrganizationId);
        Assert.AreEqual(subscription.Id, tracked.Entity.SubscriptionId);
        Assert.AreEqual(OrderStatus.Draft, tracked.Entity.Status);
    }

    [TestMethod]
    public async Task OrderStateTransition_MarkPending_PersistsStatusChange()
    {
        var plan = await SeedPlanAsync("Pro Plan", 49.99m);

        var orderResult = SubscriptionOrder.CreateNewSubscriptionOrder(OrgId, plan, FixedNow);
        Assert.IsTrue(orderResult.IsSuccess);
        var order = orderResult.Value;
        _dbContext.Add(order);
        await _dbContext.SaveChangesAsync();

        order.MarkPending();
        await _dbContext.SaveChangesAsync();

        var tracked = _dbContext.ChangeTracker.Entries<SubscriptionOrder>()
            .FirstOrDefault(e => e.Entity.Id == order.Id);
        Assert.IsNotNull(tracked);
        Assert.AreEqual(OrderStatus.Pending, tracked.Entity.Status);
    }

    [TestMethod]
    public async Task OrderStateTransition_MarkPaid_PersistsStatusChange()
    {
        var plan = await SeedPlanAsync("Pro Plan", 49.99m);

        var orderResult = SubscriptionOrder.CreateNewSubscriptionOrder(OrgId, plan, FixedNow);
        Assert.IsTrue(orderResult.IsSuccess);
        var order = orderResult.Value;
        _dbContext.Add(order);
        await _dbContext.SaveChangesAsync();

        order.MarkPending();
        order.MarkPaid();
        await _dbContext.SaveChangesAsync();

        var tracked = _dbContext.ChangeTracker.Entries<SubscriptionOrder>()
            .FirstOrDefault(e => e.Entity.Id == order.Id);
        Assert.IsNotNull(tracked);
        Assert.AreEqual(OrderStatus.Paid, tracked.Entity.Status);
    }
}
