using Viora.Domain.RealTimeScheduling;

namespace Viora.Test.Compenents.Domain.RealTimeScheduling;

/// <summary>
/// Unit tests for the ScheduleCancellations entity covering the Create factory method.
/// </summary>
[TestClass]
public sealed class ScheduleCancellationsTests
{
    /// <summary>
    /// Verifies that Create with valid input returns a ScheduleCancellations with all properties correctly set.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsCancellationWithCorrectProperties()
    {
        // Arrange
        Guid shiftId = Guid.NewGuid();
        DateTime cancellationDate = new(2026, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        string reason = "Staff sick leave";

        // Act
        ScheduleCancellations cancellation = ScheduleCancellations.Create(shiftId, cancellationDate, reason);

        // Assert
        Assert.IsNotNull(cancellation);
        Assert.AreNotEqual(Guid.Empty, cancellation.Id);
        Assert.AreEqual(shiftId, cancellation.ShiftId);
        Assert.AreEqual(cancellationDate, cancellation.CancellationDate);
        Assert.AreEqual(reason, cancellation.Reason);
    }

    /// <summary>
    /// Verifies that Create generates a new unique Id for each cancellation.
    /// </summary>
    [TestMethod]
    public void Create_CalledTwice_GeneratesDifferentIds()
    {
        // Arrange
        Guid shiftId = Guid.NewGuid();
        DateTime date = DateTime.UtcNow;

        // Act
        ScheduleCancellations c1 = ScheduleCancellations.Create(shiftId, date, "Reason A");
        ScheduleCancellations c2 = ScheduleCancellations.Create(shiftId, date, "Reason A");

        // Assert
        Assert.AreNotEqual(c1.Id, c2.Id);
    }

    /// <summary>
    /// Verifies that Create with empty reason sets an empty string.
    /// </summary>
    [TestMethod]
    public void Create_WithEmptyReason_SetsEmptyString()
    {
        // Arrange & Act
        ScheduleCancellations cancellation = ScheduleCancellations.Create(Guid.NewGuid(), DateTime.UtcNow, "");

        // Assert
        Assert.AreEqual("", cancellation.Reason);
    }

    /// <summary>
    /// Verifies that Create with a specific shift ID links it correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithSpecificShiftId_SetsShiftIdCorrectly()
    {
        // Arrange
        Guid shiftId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        // Act
        ScheduleCancellations cancellation = ScheduleCancellations.Create(shiftId, DateTime.UtcNow, "Vacation");

        // Assert
        Assert.AreEqual(shiftId, cancellation.ShiftId);
    }

    /// <summary>
    /// Verifies that Create with UTC date sets the date correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithUtcDate_SetsDateCorrectly()
    {
        // Arrange
        DateTime utcDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        ScheduleCancellations cancellation = ScheduleCancellations.Create(Guid.NewGuid(), utcDate, "Holiday");

        // Assert
        Assert.AreEqual(utcDate, cancellation.CancellationDate);
    }
}
