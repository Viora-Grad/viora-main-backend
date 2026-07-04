using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Archives.DeleteArchive;

public sealed record DeleteArchiveCommand(Guid Id) : ICommand;
