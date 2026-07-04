using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;

namespace Viora.Application.Archives.CreateFolder;

public sealed record CreateFolderCommand(
    Guid ArchiveId,
    Guid? ParentFolderId,
    string Name,
    string Description,
    string Type,
    int Order
) : ICommand<FolderResponse>;
