using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;

namespace Viora.Application.Archives.GetFolder;

public sealed record GetFolderQuery(Guid Id) : IQuery<FolderResponse>;
