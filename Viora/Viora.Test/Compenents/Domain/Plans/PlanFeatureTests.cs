using Viora.Domain.Plans;

namespace Viora.Test.Compenents.Domain.Plans;

/// <summary>
/// Unit tests for the PlanFeature join entity covering the Create factory method.
/// </summary>
[TestClass]
public sealed class PlanFeatureTests
{
    /// <summary>
    /// Verifies that Create with valid input returns a PlanFeature with Id, PlanId, and FeatureId correctly set.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsPlanFeatureWithCorrectProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid planId = Guid.NewGuid();
        Guid featureId = Guid.NewGuid();

        // Act
        PlanFeature planFeature = PlanFeature.Create(id, planId, featureId);

        // Assert
        Assert.IsNotNull(planFeature);
        Assert.AreEqual(id, planFeature.Id);
        Assert.AreEqual(planId, planFeature.PlanId);
        Assert.AreEqual(featureId, planFeature.FeatureId);
    }

    /// <summary>
    /// Verifies that specific GUID values are correctly assigned to each property.
    /// </summary>
    [TestMethod]
    public void Create_WithSpecificIds_SetsIdsCorrectly()
    {
        // Arrange
        Guid id = new("11111111-1111-1111-1111-111111111111");
        Guid planId = new("22222222-2222-2222-2222-222222222222");
        Guid featureId = new("33333333-3333-3333-3333-333333333333");

        // Act
        PlanFeature planFeature = PlanFeature.Create(id, planId, featureId);

        // Assert
        Assert.AreEqual(id, planFeature.Id);
        Assert.AreEqual(planId, planFeature.PlanId);
        Assert.AreEqual(featureId, planFeature.FeatureId);
    }

    /// <summary>
    /// Verifies that Create accepts Guid.Empty for all parameters without throwing.
    /// </summary>
    [TestMethod]
    public void Create_WithEmptyGuids_SetsEmptyGuids()
    {
        // Arrange & Act
        PlanFeature planFeature = PlanFeature.Create(Guid.Empty, Guid.Empty, Guid.Empty);

        // Assert
        Assert.AreEqual(Guid.Empty, planFeature.Id);
        Assert.AreEqual(Guid.Empty, planFeature.PlanId);
        Assert.AreEqual(Guid.Empty, planFeature.FeatureId);
    }

    /// <summary>
    /// Verifies that two PlanFeature instances created separately are not the same reference.
    /// </summary>
    [TestMethod]
    public void Create_DifferentInstances_AreNotSameReference()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid planId = Guid.NewGuid();
        Guid featureId = Guid.NewGuid();

        // Act
        PlanFeature feature1 = PlanFeature.Create(id, planId, featureId);
        PlanFeature feature2 = PlanFeature.Create(Guid.NewGuid(), planId, featureId);

        // Assert
        Assert.AreNotSame(feature1, feature2);
    }
}
