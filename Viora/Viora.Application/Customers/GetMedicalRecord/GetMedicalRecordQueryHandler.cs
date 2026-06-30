using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.MedicalRecords;

namespace Viora.Application.Customers.GetMedicalRecord;

internal class GetMedicalRecordQueryHandler(
    IMedicalRecordRepository medicalRecordRepository,
    IUserContext userContext
    ) : IQueryHandler<GetMedicalRecordQuery, MedicalRecord>
{
    public async Task<Result<MedicalRecord>> Handle(GetMedicalRecordQuery request, CancellationToken cancellationToken)
    {
        var customerId = userContext.UserId;
        var record = await medicalRecordRepository.GetByCustomerIdAsync(customerId, cancellationToken) ??
            throw new NotFoundException("Medical record not found");

        return Result.Success(record);
    }
}
