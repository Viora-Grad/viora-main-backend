using System.Text.Json;
using Viora.Domain.Abstractions;
using Viora.Domain.Forms.Internals;

namespace Viora.Domain.Forms;

public class Form : Entity
{
    public Guid ServiceId { get; private set; }
    public Guid StaffId { get; private set; }
    public FormName Name { get; private set; }
    public JsonDocument Fields { get; private set; }

    public Form() { } // For EF core 

    private Form(Guid id, Guid serviceId, Guid staffId, FormName name, JsonDocument fields) : base(id)
    {
        ServiceId = serviceId;
        StaffId = StaffId;
        Name = name;
        Fields = fields;
    }


    public static Result<Form> Create(Guid serviceId, Guid staffId, string name, JsonDocument fields)
    {
        var id = Guid.NewGuid();
        var Form = new Form(id, serviceId, staffId, FormName.create(name), fields);
        return Result.Success(Form);
    }

    public void Update(JsonDocument fields)
    {
        Fields = fields;
    }
}
