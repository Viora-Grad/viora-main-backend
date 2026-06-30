using Viora.Domain.Abstractions;

namespace Viora.Domain.Prescriptions;

public class PrescriptionError
{
    public static readonly Error FailedItem = new Error("FailedItem", "the prescription Item is failed", ErrorCategory.Validation);
    public static readonly Error PrescriptionTemplateNotFound = new Error(
        "PrescriptionTemplateNotConfigured",
        "The organization has not configured a prescription template.",
        ErrorCategory.NotFound);

}
