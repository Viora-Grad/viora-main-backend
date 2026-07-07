using Viora.Domain.Plans.Features;

namespace Viora.Test.Compenents.Domain.Plans;

/// <summary>
/// Unit tests for the LimitedFeature entity covering static instances, the All property, and singleton consistency.
/// </summary>
[TestClass]
public sealed class LimitedFeatureTests
{
    // ===== Static Instances =====

    /// <summary>
    /// Verifies that the Branches static instance has the correct Id, Key, and Description.
    /// </summary>
    [TestMethod]
    public void Branches_HasCorrectIdAndKey()
    {
        // Assert
        Assert.AreEqual(new Guid("f1a2b3c4-0001-0000-0000-000000000001"), LimitedFeature.Branches.Id);
        Assert.AreEqual("branches", LimitedFeature.Branches.Key.value);
        Assert.AreEqual("Number of branches the organization can have", LimitedFeature.Branches.Description.value);
    }

    /// <summary>
    /// Verifies that the ServicesPerBranch static instance has the correct Id, Key, and Description.
    /// </summary>
    [TestMethod]
    public void ServicesPerBranch_HasCorrectIdAndKey()
    {
        // Assert
        Assert.AreEqual(new Guid("f1a2b3c4-0002-0000-0000-000000000002"), LimitedFeature.ServicesPerBranch.Id);
        Assert.AreEqual("services_per_branch", LimitedFeature.ServicesPerBranch.Key.value);
        Assert.AreEqual("Number of services allowed per branch", LimitedFeature.ServicesPerBranch.Description.value);
    }

    /// <summary>
    /// Verifies that the StaffMembers static instance has the correct Id, Key, and Description.
    /// </summary>
    [TestMethod]
    public void StaffMembers_HasCorrectIdAndKey()
    {
        // Assert
        Assert.AreEqual(new Guid("f1a2b3c4-0003-0000-0000-000000000003"), LimitedFeature.StaffMembers.Id);
        Assert.AreEqual("staff_members", LimitedFeature.StaffMembers.Key.value);
        Assert.AreEqual("Number of staff members the organization can have", LimitedFeature.StaffMembers.Description.value);
    }

    /// <summary>
    /// Verifies that the StorageBytes static instance has the correct Id, Key, and Description.
    /// </summary>
    [TestMethod]
    public void StorageBytes_HasCorrectIdAndKey()
    {
        // Assert
        Assert.AreEqual(new Guid("f1a2b3c4-0004-0000-0000-000000000004"), LimitedFeature.StorageBytes.Id);
        Assert.AreEqual("storage_gb", LimitedFeature.StorageBytes.Key.value);
        Assert.AreEqual("Storage quota in Bytes", LimitedFeature.StorageBytes.Description.value);
    }

    // ===== All Property =====
    /// <summary>
    /// Verifies that the All collection contains the Branches instance.
    /// </summary>
    [TestMethod]
    public void All_ContainsBranches()
    {
        // Assert
        Assert.IsTrue(LimitedFeature.All.Contains(LimitedFeature.Branches));
    }

    /// <summary>
    /// Verifies that the All collection contains the ServicesPerBranch instance.
    /// </summary>
    [TestMethod]
    public void All_ContainsServicesPerBranch()
    {
        // Assert
        Assert.IsTrue(LimitedFeature.All.Contains(LimitedFeature.ServicesPerBranch));
    }

    /// <summary>
    /// Verifies that the All collection contains the StaffMembers instance.
    /// </summary>
    [TestMethod]
    public void All_ContainsStaffMembers()
    {
        // Assert
        Assert.IsTrue(LimitedFeature.All.Contains(LimitedFeature.StaffMembers));
    }

    /// <summary>
    /// Verifies that the All collection contains the StorageBytes instance.
    /// </summary>
    [TestMethod]
    public void All_ContainsStorageBytes()
    {
        // Assert
        Assert.IsTrue(LimitedFeature.All.Contains(LimitedFeature.StorageBytes));
    }

    // ===== Singleton Consistency =====

    /// <summary>
    /// Verifies that accessing static instances multiple times returns the same reference.
    /// </summary>
    [TestMethod]
    public void StaticInstances_ReturnSameReferenceOnMultipleAccess()
    {
        // Assert - static readonly fields return the same reference
        Assert.AreSame(LimitedFeature.Branches, LimitedFeature.Branches);
        Assert.AreSame(LimitedFeature.ServicesPerBranch, LimitedFeature.ServicesPerBranch);
        Assert.AreSame(LimitedFeature.StaffMembers, LimitedFeature.StaffMembers);
        Assert.AreSame(LimitedFeature.StorageBytes, LimitedFeature.StorageBytes);
    }

    /// <summary>
    /// Verifies that multiple accesses to All return collections with the same elements in the same order.
    /// </summary>
    [TestMethod]
    public void All_ReturnsSameElementsOnMultipleAccess()
    {
        // Arrange
        IReadOnlyList<LimitedFeature> first = LimitedFeature.All;
        IReadOnlyList<LimitedFeature> second = LimitedFeature.All;

        // Assert
        Assert.AreEqual(first.Count, second.Count);
        for (int i = 0; i < first.Count; i++)
        {
            Assert.AreSame(first[i], second[i]);
        }
    }
}
