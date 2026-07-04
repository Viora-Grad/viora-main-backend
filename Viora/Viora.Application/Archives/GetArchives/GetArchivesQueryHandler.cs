using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;

namespace Viora.Application.Archives.GetArchives;

internal class GetArchivesQueryHandler(
    IArchiveRepository archiveRepository) : IQueryHandler<GetArchivesQuery, List<ArchiveResponse>>
{
    public async Task<Result<List<ArchiveResponse>>> Handle(GetArchivesQuery request, CancellationToken cancellationToken)
    {
        var archives = await archiveRepository.GetByOrganizationIdAsync(request.OrganizationId, cancellationToken);

        var response = archives.Select(a => new ArchiveResponse(
            a.Id,
            a.OrganizationId,
            a.Name.Value,
            a.Description.Value,
            a.RootFolder,
            a.Setting,
            a.CreatedAt
        )).ToList();

        return Result.Success(response);
    }
}
