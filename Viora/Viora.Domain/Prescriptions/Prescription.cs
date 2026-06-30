using Viora.Domain.Abstractions;

namespace Viora.Domain.Prescriptions;

public class Prescription : Entity
{
    public Guid AppointmentId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<PrescriptionItem> items = new List<PrescriptionItem>();

    protected Prescription() { }

    private Prescription(Guid id, Guid appointmentId, DateTime createdAt) : base(id)
    {
        AppointmentId = appointmentId;
        CreatedAt = createdAt;
    }


    public static Result<Prescription> Create(Guid AppointmentId, DateTime CreatedAt)
    {
        var id = Guid.NewGuid();
        var prescription = new Prescription(id, AppointmentId, CreatedAt);
        return Result.Success(prescription);
    }
}
