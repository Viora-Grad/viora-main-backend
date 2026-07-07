using Viora.Domain.Shared;
using Viora.Domain.Subscriptions.Addons;
using Viora.Domain.Subscriptions.Addons.Internal;

namespace Viora.Test.Compenents.Domain.Subscriptions;

/// <summary>
/// Unit tests for the LimitedFeatureAddon entity covering the Create factory method.
/// </summary>
[TestClass]
public sealed class LimitedFeatureAddonTests
{
    /// <summary>
    /// Verifies that Create with valid input returns a LimitedFeatureAddon with all properties correctly set.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsLimitedFeatureAddonWithCorrectProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid limitedFeatureId = Guid.NewGuid();
        AddonType addonType = AddonType.TimeBase;
        int restoreValue = 10;
        Money price = new(25.50m, Currency.Usd);

        // Act
        LimitedFeatureAddon addon = LimitedFeatureAddon.Create(id, limitedFeatureId, addonType, restoreValue, price);

        // Assert
        Assert.IsNotNull(addon);
        Assert.AreEqual(id, addon.Id);
        Assert.AreEqual(limitedFeatureId, addon.LimitedFeatureId);
        Assert.AreEqual(addonType, addon.AddonType);
        Assert.AreEqual(restoreValue, addon.RestoreValue);
        Assert.AreEqual(price, addon.Price);
    }

    /// <summary>
    /// Verifies that Create with OneTime addon type sets the correct type.
    /// </summary>
    [TestMethod]
    public void Create_WithOneTimeAddonType_SetsCorrectType()
    {
        // Arrange & Act
        LimitedFeatureAddon addon = LimitedFeatureAddon.Create(
            Guid.NewGuid(), Guid.NewGuid(), AddonType.OneTime, 5, new Money(10m, Currency.Egp));

        // Assert
        Assert.AreEqual(AddonType.OneTime, addon.AddonType);
    }

    /// <summary>
    /// Verifies that Create with TimeBase addon type sets the correct type.
    /// </summary>
    [TestMethod]
    public void Create_WithTimeBaseAddonType_SetsCorrectType()
    {
        // Arrange & Act
        LimitedFeatureAddon addon = LimitedFeatureAddon.Create(
            Guid.NewGuid(), Guid.NewGuid(), AddonType.TimeBase, 20, new Money(50m, Currency.Usd));

        // Assert
        Assert.AreEqual(AddonType.TimeBase, addon.AddonType);
    }

    /// <summary>
    /// Verifies that Create with zero restore value sets correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithZeroRestoreValue_SetsZeroValue()
    {
        // Arrange & Act
        LimitedFeatureAddon addon = LimitedFeatureAddon.Create(
            Guid.NewGuid(), Guid.NewGuid(), AddonType.OneTime, 0, new Money(0m, Currency.Usd));

        // Assert
        Assert.AreEqual(0, addon.RestoreValue);
    }

    /// <summary>
    /// Verifies that Create with EGP currency sets the correct currency on the price.
    /// </summary>
    [TestMethod]
    public void Create_WithEgpCurrency_SetsCorrectCurrency()
    {
        // Arrange & Act
        LimitedFeatureAddon addon = LimitedFeatureAddon.Create(
            Guid.NewGuid(), Guid.NewGuid(), AddonType.TimeBase, 15, new Money(100m, Currency.Egp));

        // Assert
        Assert.AreEqual(Currency.Egp, addon.Price.Currency);
        Assert.AreEqual(100m, addon.Price.Amount);
    }

    /// <summary>
    /// Verifies that two LimitedFeatureAddon instances created with the same data are not the same reference.
    /// </summary>
    [TestMethod]
    public void Create_DifferentInstances_AreNotSameReference()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid featureId = Guid.NewGuid();

        // Act
        LimitedFeatureAddon addon1 = LimitedFeatureAddon.Create(id, featureId, AddonType.OneTime, 1, new Money(10m, Currency.Usd));
        LimitedFeatureAddon addon2 = LimitedFeatureAddon.Create(Guid.NewGuid(), featureId, AddonType.OneTime, 1, new Money(10m, Currency.Usd));

        // Assert
        Assert.AreNotSame(addon1, addon2);
    }
}
