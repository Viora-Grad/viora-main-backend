using System.Text.Json;

namespace Viora.Application.Forms.Shared;

public class FormResponse
{
    public Guid Id { get; set; }
    public Guid StaffId { get; set; }
    public Guid ServiceId { get; set; }
    public string Name { get; set; }
    public JsonDocument Fields { get; set; }



    public FormResponse(Guid id, Guid staffId, Guid serviceId, string name, JsonDocument fields)
    {
        Id = id;
        StaffId = staffId;
        ServiceId = serviceId;
        Name = name;
        Fields = fields;
    }
}
