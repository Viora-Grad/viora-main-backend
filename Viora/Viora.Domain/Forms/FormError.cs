using Viora.Domain.Abstractions;

namespace Viora.Domain.Forms;

public class FormError
{
    public static readonly Error FormConflict = new Error("FormConflict", " the service already have form ", ErrorCategory.Conflict);
}
