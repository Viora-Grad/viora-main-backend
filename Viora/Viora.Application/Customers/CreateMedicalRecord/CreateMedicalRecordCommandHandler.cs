using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.MedicalRecords;
using Viora.Domain.MedicalRecords.Internal;
using Viora.Domain.Users.Customers;

namespace Viora.Application.Customers.CreateMedicalRecord;

internal class CreateMedicalRecordCommandHandler(
    IUserContext userContext,
    IMedicalRecordRepository medicalRecordRepository,
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<CreateMedicalRecordCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateMedicalRecordCommand request, CancellationToken cancellationToken)
    {
        var customerId = userContext.UserId;
        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken) ??
            throw new NotFoundException("Customer not found");

        var medicalRecord = await medicalRecordRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        if (medicalRecord is not null)
            throw new ConflictException("Medical record already exists for this customer");

        var record = MedicalRecord.Create(
            customerId,
            new BloodPressure(request.Systolic, request.Diastolic),
            new Weight(request.Weight),
            new HeartRate(request.HeartRate),
            new BloodGlucose(request.BloodGlucose),
            request.Allergies.Select(a => new Allergy(a))
        );
        medicalRecordRepository.Add(record);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(record.Id);
    }
}
