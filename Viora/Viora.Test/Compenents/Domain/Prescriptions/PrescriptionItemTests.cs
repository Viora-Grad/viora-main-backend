using Viora.Domain.Abstractions;
using Viora.Domain.Prescriptions;

namespace Viora.Test.Compenents.Domain.Prescriptions;

/// <summary>
/// Unit tests for the PrescriptionItem entity covering the Create factory method.
/// </summary>
[TestClass]
public sealed class PrescriptionItemTests
{
    /// <summary>
    /// Verifies that Create with valid input returns a PrescriptionItem with all properties correctly set.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsItemWithCorrectProperties()
    {
        // Arrange
        Guid prescriptionId = Guid.NewGuid();
        string name = "Amoxicillin";
        string? note = "Take with food";
        string dose = "500mg";
        int frequency = 3;
        int duration = 10;

        // Act
        Result<PrescriptionItem> result = PrescriptionItem.Create(prescriptionId, name, note, dose, frequency, duration);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        PrescriptionItem item = result.Value;
        Assert.IsNotNull(item);
        Assert.AreNotEqual(Guid.Empty, item.Id);
        Assert.AreEqual(prescriptionId, item.PrescriptionId);
        Assert.AreEqual(name, item.Name.Value);
        Assert.AreEqual(note, item.Note!.Value);
        Assert.AreEqual(dose, item.Dosage.Dose);
        Assert.AreEqual(frequency, item.Dosage.Frequency);
        Assert.AreEqual(duration, item.Dosage.Duration);
    }

    /// <summary>
    /// Verifies that Create generates a new unique Id for each item.
    /// </summary>
    [TestMethod]
    public void Create_CalledTwice_GeneratesDifferentIds()
    {
        // Arrange
        Guid prescriptionId = Guid.NewGuid();

        // Act
        Result<PrescriptionItem> r1 = PrescriptionItem.Create(prescriptionId, "Drug A", null, "10mg", 1, 5);
        Result<PrescriptionItem> r2 = PrescriptionItem.Create(prescriptionId, "Drug B", null, "20mg", 2, 7);

        // Assert
        Assert.AreNotEqual(r1.Value.Id, r2.Value.Id);
    }

    /// <summary>
    /// Verifies that Create with null note sets Note's inner Value to null.
    /// </summary>
    [TestMethod]
    public void Create_WithNullNote_NoteValueIsNull()
    {
        // Arrange & Act
        Result<PrescriptionItem> result = PrescriptionItem.Create(
            Guid.NewGuid(), "Paracetamol", null, "500mg", 2, 3);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.Value.Note!.Value);
    }

    /// <summary>
    /// Verifies that Create with a specific prescription ID links it correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithSpecificPrescriptionId_SetsPrescriptionIdCorrectly()
    {
        // Arrange
        Guid prescriptionId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        // Act
        Result<PrescriptionItem> result = PrescriptionItem.Create(
            prescriptionId, "Ibuprofen", null, "400mg", 2, 5);

        // Assert
        Assert.AreEqual(prescriptionId, result.Value.PrescriptionId);
    }

    /// <summary>
    /// Verifies that Create stores the medication name correctly.
    /// </summary>
    [TestMethod]
    public void Create_StoresMedicationNameCorrectly()
    {
        // Arrange & Act
        Result<PrescriptionItem> result = PrescriptionItem.Create(
            Guid.NewGuid(), "Metformin", "Monitor blood sugar", "850mg", 2, 30);

        // Assert
        Assert.AreEqual("Metformin", result.Value.Name.Value);
    }

    /// <summary>
    /// Verifies that Create stores dosage values correctly.
    /// </summary>
    [TestMethod]
    public void Create_StoresDosageCorrectly()
    {
        // Arrange & Act
        Result<PrescriptionItem> result = PrescriptionItem.Create(
            Guid.NewGuid(), "Azithromycin", null, "250mg", 1, 5);

        // Assert
        Assert.AreEqual("250mg", result.Value.Dosage.Dose);
        Assert.AreEqual(1, result.Value.Dosage.Frequency);
        Assert.AreEqual(5, result.Value.Dosage.Duration);
    }

    /// <summary>
    /// Verifies that Create with a note stores it correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithNote_StoresNoteCorrectly()
    {
        // Arrange & Act
        Result<PrescriptionItem> result = PrescriptionItem.Create(
            Guid.NewGuid(), "Omeprazole", "Take before breakfast", "20mg", 1, 14);

        // Assert
        Assert.AreEqual("Take before breakfast", result.Value.Note!.Value);
    }
}
