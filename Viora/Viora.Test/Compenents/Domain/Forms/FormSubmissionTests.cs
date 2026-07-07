using System.Text.Json;
using Viora.Domain.Abstractions;
using Viora.Domain.Forms;

namespace Viora.Test.Compenents.Domain.Forms;

/// <summary>
/// Unit tests for the FormSubmission entity covering the Create factory method.
/// </summary>
[TestClass]
public sealed class FormSubmissionTests
{
    /// <summary>
    /// Verifies that Create with valid input returns a FormSubmission with all properties correctly set.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsSubmissionWithCorrectProperties()
    {
        // Arrange
        Guid appointmentId = Guid.NewGuid();
        Guid formId = Guid.NewGuid();
        JsonDocument submission = CreateTestSubmission();
        DateTime createdAt = new(2026, 6, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        Result<FormSubmission> result = FormSubmission.Create(appointmentId, formId, submission, createdAt);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        FormSubmission formSubmission = result.Value;
        Assert.IsNotNull(formSubmission);
        Assert.AreNotEqual(Guid.Empty, formSubmission.Id);
        Assert.AreEqual(appointmentId, formSubmission.AppointmentId);
        Assert.AreEqual(formId, formSubmission.FormId);
        Assert.AreEqual(submission, formSubmission.Submission);
        Assert.AreEqual(createdAt, formSubmission.CreatedAt);
    }

    /// <summary>
    /// Verifies that Create generates a new unique Id for each submission.
    /// </summary>
    [TestMethod]
    public void Create_CalledTwice_GeneratesDifferentIds()
    {
        // Arrange
        JsonDocument submission = CreateTestSubmission();
        DateTime now = DateTime.UtcNow;

        // Act
        Result<FormSubmission> r1 = FormSubmission.Create(Guid.NewGuid(), Guid.NewGuid(), submission, now);
        Result<FormSubmission> r2 = FormSubmission.Create(Guid.NewGuid(), Guid.NewGuid(), submission, now);

        // Assert
        Assert.AreNotEqual(r1.Value.Id, r2.Value.Id);
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
        Result<FormSubmission> result = FormSubmission.Create(
            appointmentId, Guid.NewGuid(), CreateTestSubmission(), DateTime.UtcNow);

        // Assert
        Assert.AreEqual(appointmentId, result.Value.AppointmentId);
    }

    /// <summary>
    /// Verifies that Create with a specific form ID links it correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithSpecificFormId_SetsFormIdCorrectly()
    {
        // Arrange
        Guid formId = new("b2c3d4e5-f6a7-8901-bcde-f12345678901");

        // Act
        Result<FormSubmission> result = FormSubmission.Create(
            Guid.NewGuid(), formId, CreateTestSubmission(), DateTime.UtcNow);

        // Assert
        Assert.AreEqual(formId, result.Value.FormId);
    }

    /// <summary>
    /// Verifies that Create stores the submission JSON correctly.
    /// </summary>
    [TestMethod]
    public void Create_StoresSubmissionJsonCorrectly()
    {
        // Arrange
        JsonDocument submission = JsonDocument.Parse("""{"answers": [{"q": "Age", "a": "25"}]}""");

        // Act
        Result<FormSubmission> result = FormSubmission.Create(
            Guid.NewGuid(), Guid.NewGuid(), submission, DateTime.UtcNow);

        // Assert
        Assert.AreEqual(submission, result.Value.Submission);
    }

    /// <summary>
    /// Verifies that Create with UTC date sets the CreatedAt correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithUtcDate_SetsCreatedAtCorrectly()
    {
        // Arrange
        DateTime utcDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        Result<FormSubmission> result = FormSubmission.Create(
            Guid.NewGuid(), Guid.NewGuid(), CreateTestSubmission(), utcDate);

        // Assert
        Assert.AreEqual(utcDate, result.Value.CreatedAt);
    }

    // ===== Helpers =====

    private static JsonDocument CreateTestSubmission()
    {
        return JsonDocument.Parse("""{"answers": []}""");
    }
}
