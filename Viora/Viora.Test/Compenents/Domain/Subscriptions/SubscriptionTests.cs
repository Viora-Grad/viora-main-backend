using Viora.Domain.Abstractions;
using Viora.Domain.Subscriptions;
using Viora.Domain.Subscriptions.Internal;

namespace Viora.Test.Compenents.Domain.Subscriptions;

/// <summary>
/// Unit tests for the Subscription aggregate root covering Create, Renew, ChangePlan, AddAddons, GetAddons, and Expire.
/// </summary>
[TestClass]
public sealed class SubscriptionTests
{
    // ===== Create =====

    /// <summary>
    /// Verifies that Create with valid input returns a Subscription with Active status and correct properties.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsSubscriptionWithCorrectProperties()
    {
        // Arrange
        Guid planId = Guid.NewGuid();
        Guid orgId = Guid.NewGuid();
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime end = new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        Result<Subscription> result = Subscription.Create(planId, orgId, start, end);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Subscription subscription = result.Value;
        Assert.IsNotNull(subscription);
        Assert.AreNotEqual(Guid.Empty, subscription.Id);
        Assert.AreEqual(planId, subscription.PlanId);
        Assert.AreEqual(orgId, subscription.OrganizationId);
        Assert.AreEqual(SubscriptionStatus.Active, subscription.Status);
        Assert.AreEqual(start, subscription.SubscriptionsStartTime);
        Assert.AreEqual(end, subscription.SubscriptionsEndTime);
    }

    /// <summary>
    /// Verifies that Create generates a new unique Guid for each subscription.
    /// </summary>
    [TestMethod]
    public void Create_CalledTwice_GeneratesDifferentIds()
    {
        // Arrange
        DateTime start = DateTime.UtcNow;
        DateTime end = start.AddMonths(1);

        // Act
        Result<Subscription> result1 = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), start, end);
        Result<Subscription> result2 = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), start, end);

        // Assert
        Assert.AreNotEqual(result1.Value.Id, result2.Value.Id);
    }

    /// <summary>
    /// Verifies that Create initializes the Addons collection as empty.
    /// </summary>
    [TestMethod]
    public void Create_InitializesEmptyAddonsCollection()
    {
        // Arrange
        DateTime start = DateTime.UtcNow;
        DateTime end = start.AddMonths(1);

        // Act
        Result<Subscription> result = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), start, end);

        // Assert
        Assert.IsNotNull(result.Value.Addons);
        Assert.AreEqual(0, result.Value.Addons.Count);
    }

    // ===== Renew =====

    /// <summary>
    /// Verifies that Renew sets the current subscription to Expired and returns a new Active subscription.
    /// </summary>
    [TestMethod]
    public void Renew_WhenCurrentIsActive_SetsCurrentToExpiredAndReturnsNewActive()
    {
        // Arrange
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime end = new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        Subscription subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), start, end).Value;

        DateTime newStart = new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime newEnd = new(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        Result<Subscription> result = subscription.Renew(newStart, newEnd);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(SubscriptionStatus.Expired, subscription.Status);
        Assert.AreEqual(SubscriptionStatus.Active, result.Value.Status);
        Assert.AreEqual(subscription.PlanId, result.Value.PlanId);
        Assert.AreEqual(subscription.OrganizationId, result.Value.OrganizationId);
    }

    /// <summary>
    /// Verifies that Renew when the current end time overlaps the new start time adjusts the end time.
    /// </summary>
    [TestMethod]
    public void Renew_WhenEndTimeOverlapsNewStart_AdjustsEndDate()
    {
        // Arrange
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime end = new(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc);
        Subscription subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), start, end).Value;

        DateTime newStart = new(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        DateTime newEnd = new(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc);

        // Act
        Result<Subscription> result = subscription.Renew(newStart, newEnd);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        // End time should be extended by the overlap period (15 days)
        DateTime expectedEnd = newEnd.Add(end.Subtract(newStart));
        Assert.AreEqual(expectedEnd, result.Value.SubscriptionsEndTime);
    }

    /// <summary>
    /// Verifies that Renew generates a new Id for the renewed subscription.
    /// </summary>
    [TestMethod]
    public void Renew_GeneratesNewIdForRenewedSubscription()
    {
        // Arrange
        DateTime start = DateTime.UtcNow;
        DateTime end = start.AddMonths(1);
        Subscription subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), start, end).Value;

        // Act
        Result<Subscription> result = subscription.Renew(end, end.AddMonths(1));

        // Assert
        Assert.AreNotEqual(subscription.Id, result.Value.Id);
    }

    // ===== ChangePlan =====

    /// <summary>
    /// Verifies that ChangePlan sets the current subscription to Canceled and returns a new Active subscription with the new plan.
    /// </summary>
    [TestMethod]
    public void ChangePlan_ValidInput_SetsCurrentToCanceledAndReturnsNewSubscription()
    {
        // Arrange
        Guid oldPlanId = Guid.NewGuid();
        Guid newPlanId = Guid.NewGuid();
        Guid orgId = Guid.NewGuid();
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime end = new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        Subscription subscription = Subscription.Create(oldPlanId, orgId, start, end).Value;

        DateTime newStart = new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime newEnd = new(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        Result<Subscription> result = subscription.ChangePlan(orgId, oldPlanId, newPlanId, newStart, newEnd);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(SubscriptionStatus.Canceled, subscription.Status);
        Assert.AreEqual(SubscriptionStatus.Active, result.Value.Status);
        Assert.AreEqual(newPlanId, result.Value.PlanId);
        Assert.AreEqual(orgId, result.Value.OrganizationId);
        Assert.AreEqual(newStart, result.Value.SubscriptionsStartTime);
        Assert.AreEqual(newEnd, result.Value.SubscriptionsEndTime);
    }

    /// <summary>
    /// Verifies that ChangePlan generates a new Id for the new subscription.
    /// </summary>
    [TestMethod]
    public void ChangePlan_GeneratesNewIdForNewSubscription()
    {
        // Arrange
        DateTime start = DateTime.UtcNow;
        DateTime end = start.AddMonths(1);
        Subscription subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), start, end).Value;

        // Act
        Result<Subscription> result = subscription.ChangePlan(
            subscription.OrganizationId, subscription.PlanId, Guid.NewGuid(), end, end.AddMonths(1));

        // Assert
        Assert.AreNotEqual(subscription.Id, result.Value.Id);
    }

    // ===== AddAddons =====

    /// <summary>
    /// Verifies that AddAddons with valid addon IDs adds them to the Addons collection.
    /// </summary>
    [TestMethod]
    public void AddAddons_ValidAddonIds_AddsToAddonsCollection()
    {
        // Arrange
        DateTime start = DateTime.UtcNow;
        DateTime end = start.AddMonths(1);
        Subscription subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), start, end).Value;
        List<Guid> addonIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];

        // Act
        Result result = subscription.AddAddons(addonIds);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(3, subscription.Addons.Count);
    }

    /// <summary>
    /// Verifies that AddAddons with null returns failure with InvalidAddonList error.
    /// </summary>
    [TestMethod]
    public void AddAddons_NullList_ReturnsFailure()
    {
        // Arrange
        DateTime start = DateTime.UtcNow;
        DateTime end = start.AddMonths(1);
        Subscription subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), start, end).Value;

        // Act
        Result result = subscription.AddAddons(null!);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(SubscriptionError.InvalidAddonList, result.Error);
    }

    /// <summary>
    /// Verifies that AddAddons with an empty list returns failure with InvalidAddonList error.
    /// </summary>
    [TestMethod]
    public void AddAddons_EmptyList_ReturnsFailure()
    {
        // Arrange
        DateTime start = DateTime.UtcNow;
        DateTime end = start.AddMonths(1);
        Subscription subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), start, end).Value;

        // Act
        Result result = subscription.AddAddons([]);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(SubscriptionError.InvalidAddonList, result.Error);
    }

    /// <summary>
    /// Verifies that each added addon is linked to the subscription's Id.
    /// </summary>
    [TestMethod]
    public void AddAddons_AddonsAreLinkedToSubscriptionId()
    {
        // Arrange
        DateTime start = DateTime.UtcNow;
        DateTime end = start.AddMonths(1);
        Subscription subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), start, end).Value;
        List<Guid> addonIds = [Guid.NewGuid(), Guid.NewGuid()];

        // Act
        subscription.AddAddons(addonIds);

        // Assert
        foreach (SubscriptionAddon addon in subscription.Addons)
        {
            Assert.AreEqual(subscription.Id, addon.SubscriptionId);
        }
    }

    /// <summary>
    /// Verifies that each added addon is active by default.
    /// </summary>
    [TestMethod]
    public void AddAddons_AddonsAreActiveByDefault()
    {
        // Arrange
        DateTime start = DateTime.UtcNow;
        DateTime end = start.AddMonths(1);
        Subscription subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), start, end).Value;

        // Act
        subscription.AddAddons([Guid.NewGuid()]);

        // Assert
        Assert.IsTrue(subscription.Addons.First().IsActive);
    }

    // ===== GetAddons =====

    /// <summary>
    /// Verifies that GetAddons returns the same internal list as the Addons property.
    /// </summary>
    [TestMethod]
    public void GetAddons_ReturnsSameCollectionAsAddonsProperty()
    {
        // Arrange
        DateTime start = DateTime.UtcNow;
        DateTime end = start.AddMonths(1);
        Subscription subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), start, end).Value;
        subscription.AddAddons([Guid.NewGuid(), Guid.NewGuid()]);

        // Act
        List<SubscriptionAddon> addons = subscription.GetAddons();

        // Assert
        Assert.AreEqual(subscription.Addons.Count, addons.Count);
    }

    /// <summary>
    /// Verifies that GetAddons returns an empty list when no addons have been added.
    /// </summary>
    [TestMethod]
    public void GetAddons_WhenEmpty_ReturnsEmptyList()
    {
        // Arrange
        DateTime start = DateTime.UtcNow;
        DateTime end = start.AddMonths(1);
        Subscription subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), start, end).Value;

        // Act
        List<SubscriptionAddon> addons = subscription.GetAddons();

        // Assert
        Assert.AreEqual(0, addons.Count);
    }

    // ===== Expire =====

    /// <summary>
    /// Verifies that Expire sets Status to Expired and SubscriptionsEndTime to the provided DateTime.
    /// </summary>
    [TestMethod]
    public void Expire_SetsStatusToExpiredAndEndTime()
    {
        // Arrange
        DateTime start = DateTime.UtcNow;
        DateTime end = start.AddMonths(1);
        Subscription subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), start, end).Value;
        DateTime expireTime = DateTime.UtcNow;

        // Act
        subscription.Expire(expireTime);

        // Assert
        Assert.AreEqual(SubscriptionStatus.Expired, subscription.Status);
        Assert.AreEqual(expireTime, subscription.SubscriptionsEndTime);
    }

    /// <summary>
    /// Verifies that Expire can be called on an already expired subscription without error.
    /// </summary>
    [TestMethod]
    public void Expire_CalledTwice_DoesNotThrow()
    {
        // Arrange
        DateTime start = DateTime.UtcNow;
        DateTime end = start.AddMonths(1);
        Subscription subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), start, end).Value;
        subscription.Expire(DateTime.UtcNow);

        // Act & Assert
        DateTime newExpireTime = DateTime.UtcNow.AddHours(1);
        subscription.Expire(newExpireTime);
        Assert.AreEqual(SubscriptionStatus.Expired, subscription.Status);
        Assert.AreEqual(newExpireTime, subscription.SubscriptionsEndTime);
    }
}
