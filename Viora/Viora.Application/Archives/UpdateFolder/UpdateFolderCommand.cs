using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Archives.UpdateFolder;

public sealed record UpdateFolderCommand(
    Guid Id,
    string Name,
    string Description,
    int Order
) : ICommand;
