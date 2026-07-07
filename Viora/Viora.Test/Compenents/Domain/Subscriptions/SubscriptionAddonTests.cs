using Viora.Domain.Abstractions;
using Viora.Domain.Subscriptions;

namespace Viora.Test.Compenents.Domain.Subscriptions;

/// <summary>
/// Unit tests for the SubscriptionAddon join entity covering CreateMany and SoftDelete.
/// </summary>
[TestClass]
public sealed class SubscriptionAddonTests
{
    // ===== CreateMany =====

    /// <summary>
    /// Verifies that CreateMany with valid IDs returns a list of SubscriptionAddon instances.
    /// </summary>
    [TestMethod]
    public void CreateMany_ValidIds_ReturnsListOfSubscriptionAddons()
    {
        // Arrange
        List<Guid> ids = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        Guid subscriptionId = Guid.NewGuid();

        // Act
        List<SubscriptionAddon> addons = SubscriptionAddon.CreateMany(ids, subscriptionId);

        // Assert
        Assert.IsNotNull(addons);
        Assert.AreEqual(3, addons.Count);
    }

    /// <summary>
    /// Verifies that each created addon has the correct SubscriptionId.
    /// </summary>
    [TestMethod]
    public void CreateMany_SetsSubscriptionIdCorrectly()
    {
        // Arrange
        Guid subscriptionId = Guid.NewGuid();
        List<Guid> ids = [Guid.NewGuid(), Guid.NewGuid()];

        // Act
        List<SubscriptionAddon> addons = SubscriptionAddon.CreateMany(ids, subscriptionId);

        // Assert
        foreach (SubscriptionAddon addon in addons)
        {
            Assert.AreEqual(subscriptionId, addon.SubscriptionId);
        }
    }

    /// <summary>
    /// Verifies that each created addon has the corresponding LimitedFeatureAddonId from the input list.
    /// </summary>
    [TestMethod]
    public void CreateMany_SetsLimitedFeatureAddonIdCorrectly()
    {
        // Arrange
        List<Guid> ids = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        Guid subscriptionId = Guid.NewGuid();

        // Act
        List<SubscriptionAddon> addons = SubscriptionAddon.CreateMany(ids, subscriptionId);

        // Assert
        for (int i = 0; i < ids.Count; i++)
        {
            Assert.AreEqual(ids[i], addons[i].LimitedFeatureAddonId);
        }
    }

    /// <summary>
    /// Verifies that each created addon has IsActive set to true by default.
    /// </summary>
    [TestMethod]
    public void CreateMany_SetsIsActiveToTrue()
    {
        // Arrange
        List<Guid> ids = [Guid.NewGuid(), Guid.NewGuid()];
        Guid subscriptionId = Guid.NewGuid();

        // Act
        List<SubscriptionAddon> addons = SubscriptionAddon.CreateMany(ids, subscriptionId);

        // Assert
        foreach (SubscriptionAddon addon in addons)
        {
            Assert.IsTrue(addon.IsActive);
        }
    }

    /// <summary>
    /// Verifies that each created addon has a unique Id.
    /// </summary>
    [TestMethod]
    public void CreateMany_EachAddonHasUniqueGuid()
    {
        // Arrange
        List<Guid> ids = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        Guid subscriptionId = Guid.NewGuid();

        // Act
        List<SubscriptionAddon> addons = SubscriptionAddon.CreateMany(ids, subscriptionId);

        // Assert
        List<Guid> uniqueIds = addons.Select(a => a.Id).Distinct().ToList();
        Assert.AreEqual(addons.Count, uniqueIds.Count);
    }

    /// <summary>
    /// Verifies that CreateMany with a single ID returns a list with one addon.
    /// </summary>
    [TestMethod]
    public void CreateMany_SingleId_ReturnsSingleAddon()
    {
        // Arrange
        List<Guid> ids = [Guid.NewGuid()];
        Guid subscriptionId = Guid.NewGuid();

        // Act
        List<SubscriptionAddon> addons = SubscriptionAddon.CreateMany(ids, subscriptionId);

        // Assert
        Assert.AreEqual(1, addons.Count);
    }

    // ===== SoftDelete =====

    /// <summary>
    /// Verifies that SoftDelete sets IsActive to false and returns success.
    /// </summary>
    [TestMethod]
    public void SoftDelete_SetsIsActiveToFalse()
    {
        // Arrange
        List<SubscriptionAddon> addons = SubscriptionAddon.CreateMany([Guid.NewGuid()], Guid.NewGuid());
        SubscriptionAddon addon = addons[0];

        // Act
        Result result = addon.SoftDelete();

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(addon.IsActive);
    }

    /// <summary>
    /// Verifies that SoftDelete can be called multiple times without error.
    /// </summary>
    [TestMethod]
    public void SoftDelete_CalledTwice_DoesNotThrow()
    {
        // Arrange
        List<SubscriptionAddon> addons = SubscriptionAddon.CreateMany([Guid.NewGuid()], Guid.NewGuid());
        SubscriptionAddon addon = addons[0];
        addon.SoftDelete();

        // Act & Assert
        Result result = addon.SoftDelete();
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(addon.IsActive);
    }
}
