using Viora.Domain.Abstractions;

namespace Viora.Domain.Forms;

public class FormSubmissionError
{
    public static readonly Error AlreadySubmit = new Error("Already sumbit", "the customer already submit form for this appointment", ErrorCategory.Conflict);
    public static readonly Error InvalidSubmission = new Error("Invalid Submission", "The submission JSON is invalid.", ErrorCategory.Validation);
    public static readonly Error QuestionsAreRequired = new Error(
        "QuestionsAreRequired", "The submission must contain at least one question.", ErrorCategory.Validation);

    public static readonly Error InvalidMediaId = new Error("Invalid media Id ", "The stored media identifier is invalid. ", ErrorCategory.Validation);
    public static readonly Error FileMissing = new Error("File missing", " the answer file could not be located ", ErrorCategory.NotFound);
}
