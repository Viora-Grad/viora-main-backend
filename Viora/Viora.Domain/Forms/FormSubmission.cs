using System.Text.Json;
using Viora.Domain.Abstractions;

namespace Viora.Domain.Forms;

public class FormSubmission : Entity
{
    public Guid AppointmentId { get; private set; }
    public Guid FormId { get; private set; }
    public JsonDocument Submission { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public FormSubmission() { } // For EF core 

    private FormSubmission(Guid id, Guid appointmentId, Guid formId, JsonDocument submission, DateTime createdAt) : base(id)
    {
        AppointmentId = appointmentId;
        FormId = formId;
        Submission = submission;
        CreatedAt = createdAt;
    }



    public static Result<FormSubmission> Create(Guid appointment, Guid formId, JsonDocument submission, DateTime createdAt)
    {
        var formSubmission = new FormSubmission(
            Guid.NewGuid(),
            appointment,
            formId,
            submission,
            createdAt
            );
        return Result.Success(formSubmission);
    }
}
