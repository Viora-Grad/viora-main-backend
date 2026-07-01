using System.Text.Json;
using Viora.Application.Abstractions.Media;

namespace Viora.Application.Forms.Shared;

public class FormSubmissionResponse
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid FormId { get; set; }
    public JsonDocument Answers { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<MediaResponse> FileList { get; set; } = new List<MediaResponse>();


    public FormSubmissionResponse(Guid id, Guid appointmentId, Guid formId, JsonDocument answers, DateTime createdAt, List<MediaResponse> answersList)
    {
        Id = id;
        AppointmentId = appointmentId;
        FormId = formId;
        Answers = answers;
        CreatedAt = createdAt;
        FileList = answersList;

    }
}
