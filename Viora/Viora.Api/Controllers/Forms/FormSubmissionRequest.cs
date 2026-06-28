using System.Text.Json;

namespace Viora.Api.Controllers.Forms;

public class FormSubmissionRequest
{
    public Guid FormId { get; set; }
    public JsonDocument submission { get; set; }
}
