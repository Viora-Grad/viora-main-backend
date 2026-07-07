using Viora.Domain.RealTimeScheduling;

namespace Viora.Test.Compenents.Domain.RealTimeScheduling;

/// <summary>
/// Unit tests for the Schedule entity covering the Create factory method and property initialization.
/// </summary>
[TestClass]
public sealed class ScheduleTests
{
    /// <summary>
    /// Verifies that Create with valid input returns a Schedule with BranchId and DayOfWeek correctly set.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsScheduleWithCorrectProperties()
    {
        // Arrange
        Guid branchId = Guid.NewGuid();
        DayOfWeek dayOfWeek = DayOfWeek.Monday;

        // Act
        Schedule schedule = Schedule.Create(branchId, dayOfWeek);

        // Assert
        Assert.IsNotNull(schedule);
        Assert.AreNotEqual(Guid.Empty, schedule.Id);
        Assert.AreEqual(branchId, schedule.BranchId);
        Assert.AreEqual(dayOfWeek, schedule.DayOfWeek);
    }

    /// <summary>
    /// Verifies that Create generates a new unique Id for each schedule.
    /// </summary>
    [TestMethod]
    public void Create_CalledTwice_GeneratesDifferentIds()
    {
        // Arrange
        Guid branchId = Guid.NewGuid();

        // Act
        Schedule schedule1 = Schedule.Create(branchId, DayOfWeek.Monday);
        Schedule schedule2 = Schedule.Create(branchId, DayOfWeek.Monday);

        // Assert
        Assert.AreNotEqual(schedule1.Id, schedule2.Id);
    }

    /// <summary>
    /// Verifies that Create with each DayOfWeek value sets the correct day.
    /// </summary>
    [TestMethod]
    public void Create_WithAllDaysOfWeek_SetsCorrectDay()
    {
        // Arrange & Act & Assert
        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            Schedule schedule = Schedule.Create(Guid.NewGuid(), day);
            Assert.AreEqual(day, schedule.DayOfWeek);
        }
    }

    /// <summary>
    /// Verifies that Create with a different branch ID sets it correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithDifferentBranchId_SetsCorrectBranchId()
    {
        // Arrange
        Guid branchId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        // Act
        Schedule schedule = Schedule.Create(branchId, DayOfWeek.Sunday);

        // Assert
        Assert.AreEqual(branchId, schedule.BranchId);
    }
}
