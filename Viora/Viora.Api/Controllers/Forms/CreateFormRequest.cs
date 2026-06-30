using System.Text.Json;

namespace Viora.Api.Controllers.Forms;

public class CreateFormRequest
{
    public Guid StaffId { get; set; }
    public Guid ServiceId { get; set; }
    public string name { get; set; }
    public JsonDocument fields { get; set; }
}
