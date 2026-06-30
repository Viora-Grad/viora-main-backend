using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Prescriptions;

namespace Viora.Application.Prescriptions.CreatePrescription;

internal class CreatePrescriptionCommandHandler(
    IAppointmentsRepository appointmentsRepository,
    IDateTimeProvider dateTimeProvider,
    IPrescriptionRepository prescriptionRepository,
    IPrescriptionItemRepository prescriptionItemRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreatePrescriptionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreatePrescriptionCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentsRepository.GetByIdAsync(request.AppointmentId, cancellationToken)
            ?? throw new NotFoundException($"the appointment with id {request.AppointmentId} not found");

        var prescriptionResult = Prescription.Create(request.AppointmentId, dateTimeProvider.UtcNow);

        if (prescriptionResult.IsFailure)
            return Result.Failure<Guid>(prescriptionResult.Error);

        var prescriptionItemsResult = request.PrescriptionItems.Select(pi => PrescriptionItem.Create(
            prescriptionResult.Value.Id,
            pi.Name,
            pi.Note,
            pi.Dose,
            pi.Frequence,
            pi.Duration
            )
        ).ToList();

        var result = prescriptionItemsResult.Any(pr => pr.IsFailure);

        if (result)
            return Result.Failure<Guid>(PrescriptionError.FailedItem);

        var prescriptionItems = prescriptionItemsResult.Select(pi => pi.Value).ToList();


        prescriptionRepository.Add(prescriptionResult.Value);
        prescriptionItemRepository.AddRange(prescriptionItems);
        await unitOfWork.SaveChangesAsync();

        return Result.Success(prescriptionResult.Value.Id);
    }
}
