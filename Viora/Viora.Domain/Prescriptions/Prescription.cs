using Viora.Domain.Abstractions;

namespace Viora.Domain.Prescriptions;

public class Prescription : Entity
{
    public Guid AppointmentId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<PrescriptionItem> _items = new();
    public IReadOnlyCollection<PrescriptionItem> Items => _items.AsReadOnly();

    public void AddItems(IEnumerable<PrescriptionItem> items) => _items.AddRange(items);

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
