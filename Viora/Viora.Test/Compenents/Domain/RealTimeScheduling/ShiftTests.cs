using Viora.Domain.RealTimeScheduling;

namespace Viora.Test.Compenents.Domain.RealTimeScheduling;

/// <summary>
/// Unit tests for the Shift entity covering the Create factory method.
/// </summary>
[TestClass]
public sealed class ShiftTests
{
    /// <summary>
    /// Verifies that Create with valid input returns a Shift with all properties correctly set.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsShiftWithCorrectProperties()
    {
        // Arrange
        Guid scheduleId = Guid.NewGuid();
        Guid staffId = Guid.NewGuid();
        TimeOnly startTime = new(9, 0);
        TimeOnly endTime = new(17, 0);

        // Act
        Shift shift = Shift.Create(scheduleId, startTime, endTime, staffId);

        // Assert
        Assert.IsNotNull(shift);
        Assert.AreNotEqual(Guid.Empty, shift.Id);
        Assert.AreEqual(scheduleId, shift.ScheduleId);
        Assert.AreEqual(staffId, shift.StaffId);
        Assert.AreEqual(startTime, shift.StartTime);
        Assert.AreEqual(endTime, shift.EndTime);
    }

    /// <summary>
    /// Verifies that Create generates a new unique Id for each shift.
    /// </summary>
    [TestMethod]
    public void Create_CalledTwice_GeneratesDifferentIds()
    {
        // Arrange
        TimeOnly start = new(8, 0);
        TimeOnly end = new(12, 0);

        // Act
        Shift shift1 = Shift.Create(Guid.NewGuid(), start, end, Guid.NewGuid());
        Shift shift2 = Shift.Create(Guid.NewGuid(), start, end, Guid.NewGuid());

        // Assert
        Assert.AreNotEqual(shift1.Id, shift2.Id);
    }

    /// <summary>
    /// Verifies that Create with same start and end time sets correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithSameStartAndEnd_SetsCorrectly()
    {
        // Arrange
        TimeOnly time = new(12, 0);

        // Act
        Shift shift = Shift.Create(Guid.NewGuid(), time, time, Guid.NewGuid());

        // Assert
        Assert.AreEqual(time, shift.StartTime);
        Assert.AreEqual(time, shift.EndTime);
    }

    /// <summary>
    /// Verifies that Create with specific schedule and staff IDs links them correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithSpecificIds_SetsIdsCorrectly()
    {
        // Arrange
        Guid scheduleId = new("11111111-1111-1111-1111-111111111111");
        Guid staffId = new("22222222-2222-2222-2222-222222222222");

        // Act
        Shift shift = Shift.Create(scheduleId, new TimeOnly(8, 0), new TimeOnly(16, 0), staffId);

        // Assert
        Assert.AreEqual(scheduleId, shift.ScheduleId);
        Assert.AreEqual(staffId, shift.StaffId);
    }

    /// <summary>
    /// Verifies that Create with late-night times sets correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithLateNightTimes_SetsCorrectly()
    {
        // Arrange
        TimeOnly start = new(22, 0);
        TimeOnly end = new(6, 0);

        // Act
        Shift shift = Shift.Create(Guid.NewGuid(), start, end, Guid.NewGuid());

        // Assert
        Assert.AreEqual(start, shift.StartTime);
        Assert.AreEqual(end, shift.EndTime);
    }
}
