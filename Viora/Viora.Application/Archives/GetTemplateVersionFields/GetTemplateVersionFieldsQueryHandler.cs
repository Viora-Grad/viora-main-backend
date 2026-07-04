using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;

namespace Viora.Application.Archives.GetTemplateVersionFields;

internal class GetTemplateVersionFieldsQueryHandler(
    ITemplateRepository templateRepository) : IQueryHandler<GetTemplateVersionFieldsQuery, TemplateVersionResponse>
{
    public async Task<Result<TemplateVersionResponse>> Handle(GetTemplateVersionFieldsQuery request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.TemplateId, cancellationToken)
            ?? throw new NotFoundException($"Template with id {request.TemplateId} not found");

        var version = template.Versions
            .FirstOrDefault(v => v.Version == request.Version)
            ?? throw new NotFoundException(
                $"Version {request.Version} not found for template '{template.Name.Value}'. " +
                $"Available versions: {string.Join(", ", template.Versions.Select(v => v.Version))}");

        var response = new TemplateVersionResponse(
            version.Id,
            version.TemplateId,
            version.Version,
            version.IsPublished,
            version.Fields.Select(f => new TemplateVersionFieldResponse(
                f.Id,
                f.Name.Value,
                f.Label.Value,
                f.Type,
                f.Required,
                f.Order,
                f.Validation,
                f.Layout)).ToList(),
            version.CreatedAt);

        return Result.Success(response);
    }
}
