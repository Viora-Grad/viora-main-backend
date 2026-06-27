using System.Text.Json;

namespace Viora.Api.Controllers.Form;

public class FormSubmissionRequest
{
    public Guid FormId { get; set; }
    public JsonDocument submission { get; set; }
    public IFormFileCollection FormFiles { get; set; }
}
