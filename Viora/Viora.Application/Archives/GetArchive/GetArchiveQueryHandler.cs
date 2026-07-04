using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;

namespace Viora.Application.Archives.GetArchive;

internal class GetArchiveQueryHandler(
    IArchiveRepository archiveRepository) : IQueryHandler<GetArchiveQuery, ArchiveResponse>
{
    public async Task<Result<ArchiveResponse>> Handle(GetArchiveQuery request, CancellationToken cancellationToken)
    {
        var archive = await archiveRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Archive with id {request.Id} not found");

        var response = new ArchiveResponse(
            archive.Id,
            archive.OrganizationId,
            archive.Name.Value,
            archive.Description.Value,
            archive.RootFolder,
            archive.Setting,
            archive.CreatedAt);

        return Result.Success(response);
    }
}
