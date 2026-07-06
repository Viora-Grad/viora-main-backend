using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Archives.UpdateTemplate;

public sealed record UpdateTemplateCommand(
    Guid Id,
    string Name,
    string Description
) : ICommand;
