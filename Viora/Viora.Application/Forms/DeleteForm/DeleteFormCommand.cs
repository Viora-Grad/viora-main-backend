using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Forms.DeleteForm;

public record DeleteFormCommand(Guid FormId) : ICommand;
