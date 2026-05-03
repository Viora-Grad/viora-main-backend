using Viora.Domain.Abstractions;

namespace Viora.Domain.MedicalRecords;

public static class MedicalRecordErrors
{
    public static readonly Error NotFound = new("MedicalRecord.NotFound",
        "The medical record with the specified identifier was not found", ErrorCategory.NotFound);
}
