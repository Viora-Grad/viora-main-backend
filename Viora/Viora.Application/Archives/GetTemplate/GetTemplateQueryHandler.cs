using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;

namespace Viora.Application.Archives.GetTemplate;

internal class GetTemplateQueryHandler(
    ITemplateRepository templateRepository) : IQueryHandler<GetTemplateQuery, TemplateResponse>
{
    public async Task<Result<TemplateResponse>> Handle(GetTemplateQuery request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Template with id {request.Id} not found");

        var response = new TemplateResponse(
            template.Id,
            template.ArchiveId,
            template.FolderId,
            template.Name.Value,
            template.Description.Value,
            template.CurrentVersion,
            template.CreatedAt);

        return Result.Success(response);
    }
}
