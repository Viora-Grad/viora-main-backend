using System.Text.Json;
using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Forms.UpdateForm;

public record UpdateFormCommand(Guid FormId, JsonDocument newFields) : ICommand;
