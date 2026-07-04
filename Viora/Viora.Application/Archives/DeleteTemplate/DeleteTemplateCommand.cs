using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Archives.DeleteTemplate;

public sealed record DeleteTemplateCommand(Guid Id) : ICommand;
