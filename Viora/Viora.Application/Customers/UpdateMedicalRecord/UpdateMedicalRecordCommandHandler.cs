using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.MedicalRecords;
using Viora.Domain.MedicalRecords.Internal;

namespace Viora.Application.Customers.UpdateMedicalRecord;

internal class UpdateMedicalRecordCommandHandler(
    IMedicalRecordRepository medicalRecordRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<UpdateMedicalRecordCommand>
{
    public async Task<Result> Handle(UpdateMedicalRecordCommand request, CancellationToken cancellationToken)
    {
        var customerId = userContext.UserId;

        var medicalRecord = await medicalRecordRepository.GetByCustomerIdAsync(customerId, cancellationToken) ??
            throw new NotFoundException("Medical record not found");

        var bloodPressure = (request.Systolic.HasValue && request.Diastolic.HasValue) ?
            new BloodPressure(request.Systolic.Value, request.Diastolic.Value) : null;

        var weight = request.Weight.HasValue ? new Weight(request.Weight.Value) : null;

        var heartRate = request.HeartRate.HasValue ? new HeartRate(request.HeartRate.Value) : null;

        var bloodGlucose = request.BloodGlucose.HasValue ? new BloodGlucose(request.BloodGlucose.Value) : null;

        var allergies = request.Allergies.Select(a => new Allergy(a)).ToList();

        medicalRecord.UpdateMedicalRecord(bloodPressure, weight, heartRate, bloodGlucose, allergies);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();

    }
}
