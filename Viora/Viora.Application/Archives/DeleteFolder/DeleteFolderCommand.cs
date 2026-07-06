using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Archives.DeleteFolder;

public sealed record DeleteFolderCommand(Guid Id) : ICommand;
