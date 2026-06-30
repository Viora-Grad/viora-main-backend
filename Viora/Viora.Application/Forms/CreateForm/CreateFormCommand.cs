using System.Text.Json;
using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Forms.CreateForm;

public record CreateFormCommand(Guid ServiceId, Guid StaffId, string Name, JsonDocument Fields) : ICommand<Guid>;

