using Viora.Domain.Plans.Features;

namespace Viora.Test.Compenents.Domain.Plans;

/// <summary>
/// Unit tests for the Feature entity covering the Create factory method and validation.
/// </summary>
[TestClass]
public sealed class FeatureTests
{
    /// <summary>
    /// Verifies that Create with valid input returns a Feature with Id, FeatureKey, and Description correctly set.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsFeatureWithCorrectProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string featureKey = "appointments";
        string description = "Online appointment booking";

        // Act
        Feature feature = Feature.Create(id, featureKey, description);

        // Assert
        Assert.IsNotNull(feature);
        Assert.AreEqual(id, feature.Id);
        Assert.AreEqual(featureKey, feature.FeatureKey.value);
        Assert.AreEqual(description, feature.Description.value);
    }

    /// <summary>
    /// Verifies that a specific GUID provided to Create is used as the Feature's Id.
    /// </summary>
    [TestMethod]
    public void Create_WithSpecificId_SetsIdCorrectly()
    {
        // Arrange
        Guid specificId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        // Act
        Feature feature = Feature.Create(specificId, "key", "desc");

        // Assert
        Assert.AreEqual(specificId, feature.Id);
    }

    /// <summary>
    /// Verifies that a long feature key string is stored correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithLongKey_SetsCorrectValue()
    {
        // Arrange
        string longKey = new('k', 200);

        // Act
        Feature feature = Feature.Create(Guid.NewGuid(), longKey, "desc");

        // Assert
        Assert.AreEqual(longKey, feature.FeatureKey.value);
    }

    /// <summary>
    /// Verifies that two Feature instances created with the same data are not the same reference.
    /// </summary>
    [TestMethod]
    public void Create_DifferentInstances_AreNotSameReference()
    {
        // Arrange
        string key = "feature_key";
        string desc = "description";

        // Act
        Feature f1 = Feature.Create(Guid.NewGuid(), key, desc);
        Feature f2 = Feature.Create(Guid.NewGuid(), key, desc);

        // Assert
        Assert.AreNotSame(f1, f2);
    }

    /// <summary>
    /// Verifies that Create throws ArgumentException when the feature key is empty.
    /// </summary>
    [TestMethod]
    public void Create_WithEmptyKey_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Feature.Create(Guid.NewGuid(), "", "description"));
    }

    /// <summary>
    /// Verifies that Create throws ArgumentException when the description is empty.
    /// </summary>
    [TestMethod]
    public void Create_WithEmptyDescription_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Feature.Create(Guid.NewGuid(), "key", ""));
    }
}
