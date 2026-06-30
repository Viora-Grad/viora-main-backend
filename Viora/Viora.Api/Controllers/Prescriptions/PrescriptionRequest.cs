using Viora.Application.Prescriptions.Shared;

namespace Viora.Api.Controllers.Prescriptions;

public class PrescriptionRequest
{
    public Guid AppointmentId { get; set; }

    public List<PrescriptionItemDTO> Items { get; set; } = new List<PrescriptionItemDTO>();

}
