using System.Text.Json;
using Viora.Domain.Abstractions;
using Viora.Domain.Forms;

namespace Viora.Test.Compenents.Domain.Forms;

/// <summary>
/// Unit tests for the Form entity covering the Create factory method and Update method.
/// </summary>
[TestClass]
public sealed class FormTests
{
    // ===== Create =====

    /// <summary>
    /// Verifies that Create with valid input returns a Form with all properties correctly set.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsFormWithCorrectProperties()
    {
        // Arrange
        Guid serviceId = Guid.NewGuid();
        Guid staffId = Guid.NewGuid();
        string name = "Patient Intake Form";
        JsonDocument fields = CreateTestFields();

        // Act
        Result<Form> result = Form.Create(serviceId, staffId, name, fields);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Form form = result.Value;
        Assert.IsNotNull(form);
        Assert.AreNotEqual(Guid.Empty, form.Id);
        Assert.AreEqual(serviceId, form.ServiceId);
        Assert.AreEqual(staffId, form.StaffId);
        Assert.AreEqual(name, form.Name.value);
        Assert.AreEqual(fields, form.Fields);
    }

    /// <summary>
    /// Verifies that Create with null staff ID returns a Form with StaffId set to null.
    /// </summary>
    [TestMethod]
    public void Create_WithNullStaffId_StaffIdIsNull()
    {
        // Arrange
        string name = "General Form";
        JsonDocument fields = CreateTestFields();

        // Act
        Result<Form> result = Form.Create(Guid.NewGuid(), null, name, fields);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.Value.StaffId);
    }

    /// <summary>
    /// Verifies that Create generates a new unique Id for each form.
    /// </summary>
    [TestMethod]
    public void Create_CalledTwice_GeneratesDifferentIds()
    {
        // Arrange
        JsonDocument fields = CreateTestFields();

        // Act
        Result<Form> result1 = Form.Create(Guid.NewGuid(), null, "Form A", fields);
        Result<Form> result2 = Form.Create(Guid.NewGuid(), null, "Form B", fields);

        // Assert
        Assert.AreNotEqual(result1.Value.Id, result2.Value.Id);
    }

    /// <summary>
    /// Verifies that Create with a specific service ID links it correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithSpecificServiceId_SetsServiceIdCorrectly()
    {
        // Arrange
        Guid serviceId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        JsonDocument fields = CreateTestFields();

        // Act
        Result<Form> result = Form.Create(serviceId, null, "Test Form", fields);

        // Assert
        Assert.AreEqual(serviceId, result.Value.ServiceId);
    }

    /// <summary>
    /// Verifies that Create stores the form name correctly.
    /// </summary>
    [TestMethod]
    public void Create_StoresNameCorrectly()
    {
        // Arrange
        string name = "Appointment Feedback";
        JsonDocument fields = CreateTestFields();

        // Act
        Result<Form> result = Form.Create(Guid.NewGuid(), null, name, fields);

        // Assert
        Assert.AreEqual(name, result.Value.Name.value);
    }

    // ===== Update =====

    /// <summary>
    /// Verifies that Update replaces the Fields property with the new JsonDocument.
    /// </summary>
    [TestMethod]
    public void Update_WithNewFields_ReplacesFields()
    {
        // Arrange
        Form form = CreateTestForm();
        JsonDocument newFields = JsonDocument.Parse("""{"updated": true}""");

        // Act
        form.Update(newFields);

        // Assert
        Assert.AreEqual(newFields, form.Fields);
    }

    /// <summary>
    /// Verifies that Update can be called multiple times, keeping only the latest fields.
    /// </summary>
    [TestMethod]
    public void Update_CalledMultipleTimes_KeepsLatestFields()
    {
        // Arrange
        Form form = CreateTestForm();
        JsonDocument first = JsonDocument.Parse("""{"v": 1}""");
        JsonDocument second = JsonDocument.Parse("""{"v": 2}""");
        JsonDocument third = JsonDocument.Parse("""{"v": 3}""");

        // Act
        form.Update(first);
        form.Update(second);
        form.Update(third);

        // Assert
        Assert.AreEqual(third, form.Fields);
    }

    /// <summary>
    /// Verifies that Update with an empty JSON object works correctly.
    /// </summary>
    [TestMethod]
    public void Update_WithEmptyJson_SetsEmptyJson()
    {
        // Arrange
        Form form = CreateTestForm();
        JsonDocument empty = JsonDocument.Parse("""{}""");

        // Act
        form.Update(empty);

        // Assert
        Assert.AreEqual(empty, form.Fields);
    }

    // ===== Helpers =====

    private static JsonDocument CreateTestFields()
    {
        return JsonDocument.Parse("""{"fields": [{"type": "text", "label": "Name"}]}""");
    }

    private static Form CreateTestForm()
    {
        return Form.Create(Guid.NewGuid(), null, "Test Form", CreateTestFields()).Value;
    }
}
