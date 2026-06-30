using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Prescriptions.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Prescriptions;

namespace Viora.Application.Prescriptions.GetPrescriptionById;

internal class GetPrescriptionByIdQueryHandler(
    IPrescriptionRepository prescriptionRepository
    ) : IQueryHandler<GetPrescriptionByIdQuery, PrescriptionResponse>
{
    public async Task<Result<PrescriptionResponse>> Handle(GetPrescriptionByIdQuery request, CancellationToken cancellationToken)
    {
        var prescription = await prescriptionRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"the prescription with id {request.Id} not found");
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
