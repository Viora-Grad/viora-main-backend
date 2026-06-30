namespace Viora.Application.Prescriptions.Shared;

public class PrescriptionResponse
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public DateTime CreateAt { get; set; }
    public List<PrescriptionItemDTO> Items { get; set; } = new List<PrescriptionItemDTO>();

    public PrescriptionResponse(Guid id, Guid appointmentId, DateTime createAt, List<PrescriptionItemDTO> items)
    {
        Id = id;
        AppointmentId = appointmentId;
        CreateAt = createAt;
        Items = items;
    }
}
