using Viora.Domain.Abstractions;
using Viora.Domain.Prescriptions.Internals;

namespace Viora.Domain.Prescriptions;

public class PrescriptionItem : Entity
{
    public Guid PrescriptionId { get; private set; }
    public MedicationName Name { get; private set; }
    public MedicalDosage Dosage { get; private set; }
    public PrescriptionNote? Note { get; private set; }

    protected PrescriptionItem() { }


    private PrescriptionItem(Guid id, Guid prescriptionId, string name, string? note, string dose, int Frequence, int duration) : base(id)
    {

        PrescriptionId = prescriptionId;
        Name = new MedicationName(name);
        Note = new PrescriptionNote(note);
        Dosage = new MedicalDosage(dose, Frequence, duration);
    }


    public static Result<PrescriptionItem> Create(Guid prescriptionId, string name, string? note, string dose, int frequence, int duration)
    {
        var id = Guid.NewGuid();
        var prescriptionItem = new PrescriptionItem(
            id,
            prescriptionId,
            name,
            note,
            dose,
            frequence,
            duration);

        return Result.Success(prescriptionItem);
    }
}
