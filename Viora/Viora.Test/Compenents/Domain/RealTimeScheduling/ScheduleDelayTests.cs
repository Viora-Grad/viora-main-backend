using Viora.Domain.RealTimeScheduling;
using Viora.Domain.RealTimeScheduling.Internals;

namespace Viora.Test.Compenents.Domain.RealTimeScheduling;

/// <summary>
/// Unit tests for the ScheduleDelay entity covering the Create factory method.
/// </summary>
[TestClass]
public sealed class ScheduleDelayTests
{
    /// <summary>
    /// Verifies that Create with valid input returns a ScheduleDelay with all properties correctly set.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsDelayWithCorrectProperties()
    {
        // Arrange
        Guid appointmentId = Guid.NewGuid();
        TimeOnly delayDuration = new(0, 30);
        string reason = "Traffic jam";
        DateTime occurrenceTime = new(2026, 6, 15, 9, 0, 0, DateTimeKind.Utc);
        InitiatorType initiator = InitiatorType.Client;

        // Act
        ScheduleDelay delay = ScheduleDelay.Create(appointmentId, delayDuration, reason, occurrenceTime, initiator);

        // Assert
        Assert.IsNotNull(delay);
        Assert.AreNotEqual(Guid.Empty, delay.Id);
        Assert.AreEqual(appointmentId, delay.AppointmentId);
        Assert.AreEqual(delayDuration, delay.DelayDuration);
        Assert.AreEqual(reason, delay.Reason);
        Assert.AreEqual(occurrenceTime, delay.OccurrenceTime);
        Assert.AreEqual(initiator, delay.Initiator);
    }

    /// <summary>
    /// Verifies that Create generates a new unique Id for each delay.
    /// </summary>
    [TestMethod]
    public void Create_CalledTwice_GeneratesDifferentIds()
    {
        // Arrange
        Guid appointmentId = Guid.NewGuid();
        TimeOnly duration = new(0, 15);

        // Act
        ScheduleDelay d1 = ScheduleDelay.Create(appointmentId, duration, "Reason", DateTime.UtcNow, InitiatorType.System);
        ScheduleDelay d2 = ScheduleDelay.Create(appointmentId, duration, "Reason", DateTime.UtcNow, InitiatorType.System);

        // Assert
        Assert.AreNotEqual(d1.Id, d2.Id);
    }

    /// <summary>
    /// Verifies that Create with System initiator sets Initiator correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithSystemInitiator_SetsInitiatorTypeSystem()
    {
        // Arrange & Act
        ScheduleDelay delay = ScheduleDelay.Create(
            Guid.NewGuid(), new TimeOnly(0, 10), "Delay", DateTime.UtcNow, InitiatorType.System);

        // Assert
        Assert.AreEqual(InitiatorType.System, delay.Initiator);
    }

    /// <summary>
    /// Verifies that Create with Client initiator sets Initiator correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithClientInitiator_SetsInitiatorTypeClient()
    {
        // Arrange & Act
        ScheduleDelay delay = ScheduleDelay.Create(
            Guid.NewGuid(), new TimeOnly(0, 20), "Late arrival", DateTime.UtcNow, InitiatorType.Client);

        // Assert
        Assert.AreEqual(InitiatorType.Client, delay.Initiator);
    }

    /// <summary>
    /// Verifies that Create with zero duration sets correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithZeroDuration_SetsZeroDuration()
    {
        // Arrange & Act
        ScheduleDelay delay = ScheduleDelay.Create(
            Guid.NewGuid(), new TimeOnly(0, 0), "No delay", DateTime.UtcNow, InitiatorType.System);

        // Assert
        Assert.AreEqual(new TimeOnly(0, 0), delay.DelayDuration);
    }

    /// <summary>
    /// Verifies that Create with a specific appointment ID links it correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithSpecificAppointmentId_SetsAppointmentIdCorrectly()
    {
        // Arrange
        Guid appointmentId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        // Act
        ScheduleDelay delay = ScheduleDelay.Create(
            appointmentId, new TimeOnly(0, 45), "Weather", DateTime.UtcNow, InitiatorType.System);

        // Assert
        Assert.AreEqual(appointmentId, delay.AppointmentId);
    }
}
