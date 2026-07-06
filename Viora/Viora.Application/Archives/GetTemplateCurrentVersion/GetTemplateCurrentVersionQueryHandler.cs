using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;

namespace Viora.Application.Archives.GetTemplateCurrentVersion;

internal class GetTemplateCurrentVersionQueryHandler(
    ITemplateRepository templateRepository) : IQueryHandler<GetTemplateCurrentVersionQuery, TemplateVersionResponse>
{
    public async Task<Result<TemplateVersionResponse>> Handle(GetTemplateCurrentVersionQuery request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.TemplateId, cancellationToken)
            ?? throw new NotFoundException($"Template with id {request.TemplateId} not found");

        if (template.Versions.Count == 0)
            throw new NotFoundException(
                $"Template '{template.Name.Value}' has no versions. " +
                $"Please delete and recreate this template with the latest version of the application.");

        var currentVersion = template.Versions
            .OrderByDescending(v => v.Version)
            .FirstOrDefault(v => v.Version == template.CurrentVersion)
            ?? throw new NotFoundException(
                $"Current version {template.CurrentVersion} not found for template '{template.Name.Value}'. " +
                $"Available versions: {string.Join(", ", template.Versions.Select(v => v.Version))}");

        var response = new TemplateVersionResponse(
            currentVersion.Id,
            currentVersion.TemplateId,
            currentVersion.Version,
            currentVersion.IsPublished,
            currentVersion.Fields.Select(f => new TemplateVersionFieldResponse(
                f.Id,
                f.Name.Value,
                f.Label.Value,
                f.Type,
                f.Required,
                f.Order,
                f.Validation,
                f.Layout)).ToList(),
            currentVersion.CreatedAt);

        return Result.Success(response);
    }
}
