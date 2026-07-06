using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;

namespace Viora.Application.Archives.GetArchive;

public sealed record GetArchiveQuery(Guid Id) : IQuery<ArchiveResponse>;
