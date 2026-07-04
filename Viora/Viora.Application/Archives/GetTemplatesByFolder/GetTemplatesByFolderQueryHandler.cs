using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;

namespace Viora.Application.Archives.GetTemplatesByFolder;

internal class GetTemplatesByFolderQueryHandler(
    ITemplateRepository templateRepository) : IQueryHandler<GetTemplatesByFolderQuery, List<TemplateResponse>>
{
    public async Task<Result<List<TemplateResponse>>> Handle(GetTemplatesByFolderQuery request, CancellationToken cancellationToken)
    {
        var templates = await templateRepository.GetByFolderIdAsync(request.FolderId, cancellationToken);

        var response = templates.Select(t => new TemplateResponse(
            t.Id,
            t.ArchiveId,
            t.FolderId,
            t.Name.Value,
            t.Description.Value,
            t.CurrentVersion,
            t.CreatedAt
        )).ToList();

        return Result.Success(response);
    }
}
