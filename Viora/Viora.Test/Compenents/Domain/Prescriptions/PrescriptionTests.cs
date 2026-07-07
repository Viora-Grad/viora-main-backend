using Viora.Domain.Abstractions;
using Viora.Domain.Prescriptions;

namespace Viora.Test.Compenents.Domain.Prescriptions;

/// <summary>
/// Unit tests for the Prescription aggregate root covering the Create factory method and AddItems.
/// </summary>
[TestClass]
public sealed class PrescriptionTests
{
    // ===== Create =====

    /// <summary>
    /// Verifies that Create with valid input returns a Prescription with correct properties.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsPrescriptionWithCorrectProperties()
    {
        // Arrange
        Guid appointmentId = Guid.NewGuid();
        DateTime createdAt = new(2026, 6, 15, 10, 0, 0, DateTimeKind.Utc);

        // Act
        Result<Prescription> result = Prescription.Create(appointmentId, createdAt);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Prescription prescription = result.Value;
        Assert.IsNotNull(prescription);
        Assert.AreNotEqual(Guid.Empty, prescription.Id);
        Assert.AreEqual(appointmentId, prescription.AppointmentId);
        Assert.AreEqual(createdAt, prescription.CreatedAt);
    }

    /// <summary>
    /// Verifies that Create generates a new unique Id for each prescription.
    /// </summary>
    [TestMethod]
    public void Create_CalledTwice_GeneratesDifferentIds()
    {
        // Arrange
        DateTime now = DateTime.UtcNow;

        // Act
        Result<Prescription> r1 = Prescription.Create(Guid.NewGuid(), now);
        Result<Prescription> r2 = Prescription.Create(Guid.NewGuid(), now);

        // Assert
        Assert.AreNotEqual(r1.Value.Id, r2.Value.Id);
    }

    /// <summary>
    /// Verifies that Create initializes the Items collection as empty.
    /// </summary>
    [TestMethod]
    public void Create_InitializesEmptyItemsCollection()
    {
        // Arrange & Act
        Result<Prescription> result = Prescription.Create(Guid.NewGuid(), DateTime.UtcNow);

        // Assert
        Assert.IsNotNull(result.Value.Items);
        Assert.AreEqual(0, result.Value.Items.Count);
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
        Result<Prescription> result = Prescription.Create(appointmentId, DateTime.UtcNow);

        // Assert
        Assert.AreEqual(appointmentId, result.Value.AppointmentId);
    }

    // ===== AddItems =====

    /// <summary>
    /// Verifies that AddItems adds multiple items to the Items collection.
    /// </summary>
    [TestMethod]
    public void AddItems_MultipleItems_AddsToCollection()
    {
        // Arrange
        Prescription prescription = Prescription.Create(Guid.NewGuid(), DateTime.UtcNow).Value;
        List<PrescriptionItem> items =
        [
            CreateTestItem(prescription.Id),
            CreateTestItem(prescription.Id),
            CreateTestItem(prescription.Id)
        ];

        // Act
        prescription.AddItems(items);

        // Assert
        Assert.AreEqual(3, prescription.Items.Count);
    }

    /// <summary>
    /// Verifies that AddItems with a single item adds it to the collection.
    /// </summary>
    [TestMethod]
    public void AddItems_SingleItem_AddsToCollection()
    {
        // Arrange
        Prescription prescription = Prescription.Create(Guid.NewGuid(), DateTime.UtcNow).Value;
        List<PrescriptionItem> items = [CreateTestItem(prescription.Id)];

        // Act
        prescription.AddItems(items);

        // Assert
        Assert.AreEqual(1, prescription.Items.Count);
    }

    /// <summary>
    /// Verifies that AddItems called multiple times accumulates all items.
    /// </summary>
    [TestMethod]
    public void AddItems_CalledMultipleTimes_AccumulatesItems()
    {
        // Arrange
        Prescription prescription = Prescription.Create(Guid.NewGuid(), DateTime.UtcNow).Value;

        // Act
        prescription.AddItems([CreateTestItem(prescription.Id)]);
        prescription.AddItems([CreateTestItem(prescription.Id), CreateTestItem(prescription.Id)]);

        // Assert
        Assert.AreEqual(3, prescription.Items.Count);
    }

    /// <summary>
    /// Verifies that Items are read-only via the IReadOnlyCollection interface.
    /// </summary>
    [TestMethod]
    public void Items_ReturnsReadOnlyCollection()
    {
        // Arrange
        Prescription prescription = Prescription.Create(Guid.NewGuid(), DateTime.UtcNow).Value;

        // Act & Assert
        Assert.IsInstanceOfType(prescription.Items, typeof(IReadOnlyCollection<PrescriptionItem>));
    }

    // ===== Helpers =====

    private static PrescriptionItem CreateTestItem(Guid prescriptionId)
    {
        return PrescriptionItem.Create(prescriptionId, "Ibuprofen", "Take after meals", "200mg", 3, 7).Value;
    }
}
