using Viora.Domain.Abstractions;
using Viora.Domain.Prescriptions;

namespace Viora.Test.Compenents.Domain.Prescriptions;

/// <summary>
/// Unit tests for the PrescriptionTemplate entity covering the Create factory method.
/// </summary>
[TestClass]
public sealed class PrescriptionTemplateTests
{
    /// <summary>
    /// Verifies that Create with valid input returns a PrescriptionTemplate with all properties correctly set.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsTemplateWithCorrectProperties()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        string name = "Clinic Prescription Template";
        Guid? mediaId = Guid.NewGuid();
        double top = 1.5;
        double right = 2.0;
        double left = 2.0;
        double bottom = 1.0;

        // Act
        Result<PrescriptionTemplate> result = PrescriptionTemplate.Create(orgId, name, mediaId, top, right, left, bottom);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        PrescriptionTemplate template = result.Value;
        Assert.IsNotNull(template);
        Assert.AreNotEqual(Guid.Empty, template.Id);
        Assert.AreEqual(orgId, template.OrganizationId);
        Assert.AreEqual(name, template.Name.Value);
        Assert.AreEqual(mediaId, template.TemplateMediaId);
        Assert.AreEqual(top, template.TopMargin);
        Assert.AreEqual(right, template.RightMargin);
        Assert.AreEqual(left, template.LeftMargin);
        Assert.AreEqual(bottom, template.BottomMargin);
    }

    /// <summary>
    /// Verifies that Create generates a new unique Id for each template.
    /// </summary>
    [TestMethod]
    public void Create_CalledTwice_GeneratesDifferentIds()
    {
        // Act
        Result<PrescriptionTemplate> r1 = PrescriptionTemplate.Create(Guid.NewGuid(), "Template A", null, 1, 1, 1, 1);
        Result<PrescriptionTemplate> r2 = PrescriptionTemplate.Create(Guid.NewGuid(), "Template B", null, 1, 1, 1, 1);

        // Assert
        Assert.AreNotEqual(r1.Value.Id, r2.Value.Id);
    }

    /// <summary>
    /// Verifies that Create with null TemplateMediaId sets it to null.
    /// </summary>
    [TestMethod]
    public void Create_WithNullMediaId_MediaIdIsNull()
    {
        // Arrange & Act
        Result<PrescriptionTemplate> result = PrescriptionTemplate.Create(
            Guid.NewGuid(), "No Media Template", null, 1, 1, 1, 1);

        // Assert
        Assert.IsNull(result.Value.TemplateMediaId);
    }

    /// <summary>
    /// Verifies that Create with a specific organization ID links it correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithSpecificOrgId_SetsOrgIdCorrectly()
    {
        // Arrange
        Guid orgId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        // Act
        Result<PrescriptionTemplate> result = PrescriptionTemplate.Create(
            orgId, "Test Template", null, 1, 1, 1, 1);

        // Assert
        Assert.AreEqual(orgId, result.Value.OrganizationId);
    }

    /// <summary>
    /// Verifies that Create stores the template name correctly.
    /// </summary>
    [TestMethod]
    public void Create_StoresTemplateNameCorrectly()
    {
        // Arrange & Act
        Result<PrescriptionTemplate> result = PrescriptionTemplate.Create(
            Guid.NewGuid(), "Hospital Standard", null, 1, 1, 1, 1);

        // Assert
        Assert.AreEqual("Hospital Standard", result.Value.Name.Value);
    }

    /// <summary>
    /// Verifies that Create with zero margins sets them correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithZeroMargins_SetsZeroValues()
    {
        // Arrange & Act
        Result<PrescriptionTemplate> result = PrescriptionTemplate.Create(
            Guid.NewGuid(), "Minimal Template", null, 0, 0, 0, 0);

        // Assert
        Assert.AreEqual(0, result.Value.TopMargin);
        Assert.AreEqual(0, result.Value.RightMargin);
        Assert.AreEqual(0, result.Value.LeftMargin);
        Assert.AreEqual(0, result.Value.BottomMargin);
    }

    /// <summary>
    /// Verifies that Create with specific media ID stores it correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithSpecificMediaId_StoresMediaIdCorrectly()
    {
        // Arrange
        Guid mediaId = new("b2c3d4e5-f6a7-8901-bcde-f12345678901");

        // Act
        Result<PrescriptionTemplate> result = PrescriptionTemplate.Create(
            Guid.NewGuid(), "Media Template", mediaId, 1, 1, 1, 1);

        // Assert
        Assert.AreEqual(mediaId, result.Value.TemplateMediaId);
    }
}
