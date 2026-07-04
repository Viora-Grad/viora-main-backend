using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;
using Viora.Domain.Archives.Internals;

namespace Viora.Application.Archives.CreateTemplateVersion;

internal class CreateTemplateVersionCommandHandler(
    ITemplateRepository templateRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateTemplateVersionCommand, TemplateVersionResponse>
{
    public async Task<Result<TemplateVersionResponse>> Handle(CreateTemplateVersionCommand request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.TemplateId, cancellationToken)
            ?? throw new NotFoundException($"Template with id {request.TemplateId} not found");

        var nextVersion = template.CurrentVersion + 1;
        var version = TemplateVersion.Create(template.Id, nextVersion, dateTimeProvider.UtcNow);

        foreach (var fieldDto in request.Fields)
        {
            var field = MapToDomain(fieldDto, version.Id);
            version.AddField(field);
        }

        template.AddVersion(version);
        templateRepository.Update(template);

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

    private static TemplateField MapToDomain(TemplateFieldDto dto, Guid templateVersionId)
    {
        var validation = dto.Validation is not null
            ? new FieldValidation(
                dto.Validation.Required,
                dto.Validation.MinLength,
                dto.Validation.MaxLength,
                dto.Validation.Min,
                dto.Validation.Max,
                dto.Validation.Regex)
            : new FieldValidation(dto.Required, null, null, null, null, null);

        var layout = dto.Layout is not null
            ? new FieldLayout(
                dto.Layout.Column,
                dto.Layout.Order,
                dto.Layout.Tab,
                dto.Layout.Width)
            : new FieldLayout(0, dto.Order, null, 12);

        return TemplateField.Create(
            templateVersionId,
            new TemplateName(dto.Name),
            new TemplateFieldLabel(dto.Label),
            dto.Type,
            dto.Required,
            dto.Order,
            validation,
            layout);
    }
}
