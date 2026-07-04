using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;

namespace Viora.Application.Archives.GetArchives;

public sealed record GetArchivesQuery(Guid OrganizationId) : IQuery<List<ArchiveResponse>>;
