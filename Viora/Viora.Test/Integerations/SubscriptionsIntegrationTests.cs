using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
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
public sealed class SubscriptionsIntegrationTests
{
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly ApplicationDbContext _dbContext;
    private static readonly Guid OrgId = Guid.NewGuid();
    private static readonly DateTime FixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

    public SubscriptionsIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options, _publisherMock.Object);
    }

    [TestCleanup]
    public void Cleanup() => _dbContext.Dispose();

    [TestMethod]
    public async Task CreateSubscription_WithPlan_PersistsActiveSubscription()
    {
        var plan = Plan.Create(Guid.NewGuid(), "Pro Plan", "Professional", "Full access", 49.99m, Currency.Usd, PlanPeriod.monthly);
        _dbContext.Add(plan);
        await _dbContext.SaveChangesAsync();

        var subResult = Subscription.Create(plan.Id, OrgId, FixedNow, FixedNow.AddMonths(1));
        Assert.IsTrue(subResult.IsSuccess);

        var subscription = subResult.Value;
        _dbContext.Add(subscription);
        await _dbContext.SaveChangesAsync();

        var retrieved = await _dbContext.Set<global::Viora.Domain.Subscriptions.Subscription>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == subscription.Id);
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(plan.Id, retrieved.PlanId);
        Assert.AreEqual(OrgId, retrieved.OrganizationId);
        Assert.AreEqual(SubscriptionStatus.Active, retrieved.Status);
        Assert.AreEqual(FixedNow, retrieved.SubscriptionsStartTime);
        Assert.AreEqual(FixedNow.AddMonths(1), retrieved.SubscriptionsEndTime);
    }

    [TestMethod]
    public async Task AddAddons_ToSubscription_PersistsAddonRelationships()
    {
        var plan = Plan.Create(Guid.NewGuid(), "Pro Plan", "Professional", "Full", 49.99m, Currency.Usd, PlanPeriod.monthly);
        _dbContext.Add(plan);
        await _dbContext.SaveChangesAsync();

        var addon = LimitedFeatureAddon.Create(Guid.NewGuid(), LimitedFeature.Branches.Id, AddonType.OneTime, 5, new Money(10m, Currency.Usd));
        _dbContext.Add(addon);
        await _dbContext.SaveChangesAsync();

        var subResult = Subscription.Create(plan.Id, OrgId, FixedNow, FixedNow.AddMonths(1));
        Assert.IsTrue(subResult.IsSuccess);
        var subscription = subResult.Value;
        _dbContext.Add(subscription);
        await _dbContext.SaveChangesAsync();

        var subAddon = SubscriptionAddon.CreateMany(new List<Guid> { addon.Id }, subscription.Id);
        _dbContext.AddRange(subAddon);
        await _dbContext.SaveChangesAsync();

        var trackedAddons = _dbContext.ChangeTracker.Entries<SubscriptionAddon>()
            .Where(e => e.Entity.SubscriptionId == subscription.Id)
            .ToList();
        Assert.AreEqual(1, trackedAddons.Count);
        Assert.AreEqual(addon.Id, trackedAddons[0].Entity.LimitedFeatureAddonId);
        Assert.IsTrue(trackedAddons[0].Entity.IsActive);
    }

    [TestMethod]
    public async Task ExpireSubscription_ChangesStatusToExpired()
    {
        var plan = Plan.Create(Guid.NewGuid(), "Pro Plan", "Professional", "Full", 49.99m, Currency.Usd, PlanPeriod.monthly);
        _dbContext.Add(plan);
        await _dbContext.SaveChangesAsync();

        var subResult = Subscription.Create(plan.Id, OrgId, FixedNow, FixedNow.AddMonths(1));
        Assert.IsTrue(subResult.IsSuccess);
        var subscription = subResult.Value;
        _dbContext.Add(subscription);
        await _dbContext.SaveChangesAsync();

        subscription.Expire(FixedNow.AddDays(35));
        await _dbContext.SaveChangesAsync();

        var retrieved = await _dbContext.Set<global::Viora.Domain.Subscriptions.Subscription>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == subscription.Id);
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(SubscriptionStatus.Expired, retrieved.Status);
    }

    [TestMethod]
    public async Task RenewSubscription_CreatesNewSubscriptionWithUpdatedPeriod()
    {
        var plan = Plan.Create(Guid.NewGuid(), "Pro Plan", "Professional", "Full", 49.99m, Currency.Usd, PlanPeriod.monthly);
        _dbContext.Add(plan);
        await _dbContext.SaveChangesAsync();

        var subResult = Subscription.Create(plan.Id, OrgId, FixedNow, FixedNow.AddMonths(1));
        Assert.IsTrue(subResult.IsSuccess);
        var subscription = subResult.Value;
        _dbContext.Add(subscription);
        await _dbContext.SaveChangesAsync();

        var renewResult = subscription.Renew(FixedNow.AddMonths(1), FixedNow.AddMonths(2));
        Assert.IsTrue(renewResult.IsSuccess);

        var newSubscription = renewResult.Value;
        _dbContext.Add(newSubscription);
        await _dbContext.SaveChangesAsync();

        var trackedSubscriptions = _dbContext.ChangeTracker.Entries<global::Viora.Domain.Subscriptions.Subscription>()
            .Where(e => e.Entity.OrganizationId == OrgId)
            .ToList();
        Assert.AreEqual(2, trackedSubscriptions.Count);
    }

    [TestMethod]
    public async Task SoftDeleteAddon_DeactivatesAddon()
    {
        var plan = Plan.Create(Guid.NewGuid(), "Pro Plan", "Professional", "Full", 49.99m, Currency.Usd, PlanPeriod.monthly);
        _dbContext.Add(plan);
        await _dbContext.SaveChangesAsync();

        var addon = LimitedFeatureAddon.Create(Guid.NewGuid(), LimitedFeature.Branches.Id, AddonType.OneTime, 5, new Money(10m, Currency.Usd));
        _dbContext.Add(addon);
        await _dbContext.SaveChangesAsync();

        var subResult = Subscription.Create(plan.Id, OrgId, FixedNow, FixedNow.AddMonths(1));
        Assert.IsTrue(subResult.IsSuccess);
        var subscription = subResult.Value;
        _dbContext.Add(subscription);
        await _dbContext.SaveChangesAsync();

        var subAddons = SubscriptionAddon.CreateMany(new List<Guid> { addon.Id }, subscription.Id);
        _dbContext.AddRange(subAddons);
        await _dbContext.SaveChangesAsync();

        subAddons[0].SoftDelete();
        await _dbContext.SaveChangesAsync();

        var tracked = _dbContext.ChangeTracker.Entries<SubscriptionAddon>()
            .FirstOrDefault(e => e.Entity.Id == subAddons[0].Id);
        Assert.IsNotNull(tracked);
        Assert.IsFalse(tracked.Entity.IsActive);
    }
}
