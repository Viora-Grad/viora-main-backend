using Viora.Domain.Abstractions;

namespace Viora.Domain.Users;

public static class CustomerErrors
{
    public static Error MedicalRecordAlreadyExists => new("Customer.MedicalRecordAlreadyExists",
        "Medical record already exists for this customer", ErrorCategory.Validation);
}
