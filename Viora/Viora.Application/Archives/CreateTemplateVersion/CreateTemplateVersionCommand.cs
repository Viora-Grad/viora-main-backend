using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;

namespace Viora.Application.Archives.CreateTemplateVersion;

public sealed record CreateTemplateVersionCommand(
    Guid TemplateId,
    List<TemplateFieldDto> Fields
) : ICommand<TemplateVersionResponse>;
