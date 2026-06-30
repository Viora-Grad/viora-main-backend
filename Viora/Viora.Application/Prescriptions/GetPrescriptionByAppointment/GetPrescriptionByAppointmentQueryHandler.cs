using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Prescriptions.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Prescriptions;

namespace Viora.Application.Prescriptions.GetPrescriptionByAppointment;

internal class GetPrescriptionByAppointmentQueryHandler(
    IAppointmentsRepository appointmentsRepository,
    IPrescriptionRepository prescriptionRepository
    ) : IQueryHandler<GetPrescriptionByAppointmentQuery, PrescriptionResponse>
{
    public async Task<Result<PrescriptionResponse>> Handle(GetPrescriptionByAppointmentQuery request, CancellationToken cancellationToken)
    {
        var appiontment = await appointmentsRepository.GetByIdAsync(request.AppointmentId, cancellationToken)
            ?? throw new NotFoundException($"the appointment with id {request.AppointmentId} not found");

        var prescription = await prescriptionRepository.GetByAppointmentIdAsync(request.AppointmentId, cancellationToken)
            ?? throw new NotFoundException($"the appointment with id {request.AppointmentId} does not have prescription");

        var prescriptionResponse = new PrescriptionResponse(
            prescription.Id,
            prescription.AppointmentId,
            prescription.CreatedAt,
            prescription.items.Select(
                i => new PrescriptionItemDTO(
                    i.Name.Value,
                    i.Note?.Value,
                    i.Dosage.Dose,
                    i.Dosage.Frequency,
                    i.Dosage.Duration
                    )
                ).ToList()
            );

        return Result.Success(prescriptionResponse);
    }
}
