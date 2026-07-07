using Viora.Domain.Plans;

namespace Viora.Test.Compenents.Domain.Plans;

/// <summary>
/// Unit tests for the PlanLimitedFeature join entity covering the Create factory method.
/// </summary>
[TestClass]
public sealed class PlanLimitedFeatureTests
{
    /// <summary>
    /// Verifies that Create with valid input returns a PlanLimitedFeature with all properties correctly set.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsPlanLimitedFeatureWithCorrectProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid planId = Guid.NewGuid();
        Guid limitedFeatureId = Guid.NewGuid();
        long limitValue = 5;

        // Act
        PlanLimitedFeature planLimitedFeature = PlanLimitedFeature.Create(id, planId, limitedFeatureId, limitValue);

        // Assert
        Assert.IsNotNull(planLimitedFeature);
        Assert.AreEqual(id, planLimitedFeature.Id);
        Assert.AreEqual(planId, planLimitedFeature.PlanId);
        Assert.AreEqual(limitedFeatureId, planLimitedFeature.LimitedFeatureId);
        Assert.AreEqual(limitValue, planLimitedFeature.LimitValue);
    }

    /// <summary>
    /// Verifies that a PlanLimitedFeature can be created with a zero limit value.
    /// </summary>
    [TestMethod]
    public void Create_WithZeroLimit_SetsZeroLimitValue()
    {
        // Arrange & Act
        PlanLimitedFeature feature = PlanLimitedFeature.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0);

        // Assert
        Assert.AreEqual(0, feature.LimitValue);
    }

    /// <summary>
    /// Verifies that a PlanLimitedFeature can be created with long.MaxValue as the limit.
    /// </summary>
    [TestMethod]
    public void Create_WithLargeLimit_SetsCorrectLimitValue()
    {
        // Arrange & Act
        PlanLimitedFeature feature = PlanLimitedFeature.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), long.MaxValue);

        // Assert
        Assert.AreEqual(long.MaxValue, feature.LimitValue);
    }

    /// <summary>
    /// Verifies that specific GUID values are correctly assigned to Id, PlanId, and LimitedFeatureId.
    /// </summary>
    [TestMethod]
    public void Create_WithSpecificIds_SetsIdsCorrectly()
    {
        // Arrange
        Guid id = new("11111111-1111-1111-1111-111111111111");
        Guid planId = new("22222222-2222-2222-2222-222222222222");
        Guid limitedFeatureId = new("33333333-3333-3333-3333-333333333333");

        // Act
        PlanLimitedFeature feature = PlanLimitedFeature.Create(id, planId, limitedFeatureId, 10);

        // Assert
        Assert.AreEqual(id, feature.Id);
        Assert.AreEqual(planId, feature.PlanId);
        Assert.AreEqual(limitedFeatureId, feature.LimitedFeatureId);
    }

    /// <summary>
    /// Verifies that two PlanLimitedFeature instances created separately are not the same reference.
    /// </summary>
    [TestMethod]
    public void Create_DifferentInstances_AreNotSameReference()
    {
        // Arrange
        Guid planId = Guid.NewGuid();
        Guid limitedFeatureId = Guid.NewGuid();

        // Act
        PlanLimitedFeature feature1 = PlanLimitedFeature.Create(Guid.NewGuid(), planId, limitedFeatureId, 5);
        PlanLimitedFeature feature2 = PlanLimitedFeature.Create(Guid.NewGuid(), planId, limitedFeatureId, 5);

        // Assert
        Assert.AreNotSame(feature1, feature2);
    }
}
