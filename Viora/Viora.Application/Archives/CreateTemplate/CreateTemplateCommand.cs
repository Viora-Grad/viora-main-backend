using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;

namespace Viora.Application.Archives.CreateTemplate;

public sealed record CreateTemplateCommand(
    Guid ArchiveId,
    Guid FolderId,
    string Name,
    string Description
) : ICommand<TemplateResponse>;
