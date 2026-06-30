using Viora.Application.Abstractions.Messaging;
using Viora.Application.Prescriptions.Shared;

namespace Viora.Application.Prescriptions.CreatePrescription;

public record CreatePrescriptionCommand(Guid AppointmentId, List<PrescriptionItemDTO> PrescriptionItems) : ICommand<Guid>;
