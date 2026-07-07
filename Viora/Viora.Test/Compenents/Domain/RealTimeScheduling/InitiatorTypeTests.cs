using Viora.Domain.RealTimeScheduling.Internals;

namespace Viora.Test.Compenents.Domain.RealTimeScheduling;

/// <summary>
/// Unit tests for the InitiatorType value object covering static instances and FromValue.
/// </summary>
[TestClass]
public sealed class InitiatorTypeTests
{
    /// <summary>
    /// Verifies that the System static instance has the correct value.
    /// </summary>
    [TestMethod]
    public void System_HasCorrectValue()
    {
        // Assert
        Assert.AreEqual("System", InitiatorType.System.Value);
    }

    /// <summary>
    /// Verifies that the Client static instance has the correct value.
    /// </summary>
    [TestMethod]
    public void Client_HasCorrectValue()
    {
        // Assert
        Assert.AreEqual("Client", InitiatorType.Client.Value);
    }

    /// <summary>
    /// Verifies that FromValue with "System" returns the System instance.
    /// </summary>
    [TestMethod]
    public void FromValue_System_ReturnsSystemInstance()
    {
        // Act
        InitiatorType result = InitiatorType.FromValue("System");

        // Assert
        Assert.AreEqual(InitiatorType.System, result);
    }

    /// <summary>
    /// Verifies that FromValue with "Client" returns the Client instance.
    /// </summary>
    [TestMethod]
    public void FromValue_Client_ReturnsClientInstance()
    {
        // Act
        InitiatorType result = InitiatorType.FromValue("Client");

        // Assert
        Assert.AreEqual(InitiatorType.Client, result);
    }

    /// <summary>
    /// Verifies that FromValue with an invalid value throws InvalidOperationException.
    /// </summary>
    [TestMethod]
    public void FromValue_InvalidValue_ThrowsInvalidOperationException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => InitiatorType.FromValue("Invalid"));
    }

    /// <summary>
    /// Verifies that System and Client are not the same reference.
    /// </summary>
    [TestMethod]
    public void SystemAndClient_AreDifferentInstances()
    {
        // Assert
        Assert.AreNotSame(InitiatorType.System, InitiatorType.Client);
        Assert.AreNotEqual(InitiatorType.System, InitiatorType.Client);
    }

    /// <summary>
    /// Verifies that accessing System multiple times returns the same reference.
    /// </summary>
    [TestMethod]
    public void System_ReturnsSameReferenceOnMultipleAccess()
    {
        // Assert
        Assert.AreSame(InitiatorType.System, InitiatorType.System);
    }

    /// <summary>
    /// Verifies that accessing Client multiple times returns the same reference.
    /// </summary>
    [TestMethod]
    public void Client_ReturnsSameReferenceOnMultipleAccess()
    {
        // Assert
        Assert.AreSame(InitiatorType.Client, InitiatorType.Client);
    }
}
