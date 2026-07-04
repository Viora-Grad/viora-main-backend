using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;
using Viora.Domain.Archives.Internals;

namespace Viora.Application.Archives.CreateRecord;

internal class CreateRecordCommandHandler(
    IRecordRepository recordRepository,
    ITemplateRepository templateRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateRecordCommand, RecordResponse>
{
    public async Task<Result<RecordResponse>> Handle(CreateRecordCommand request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.TemplateId, cancellationToken)
            ?? throw new NotFoundException($"Template with id {request.TemplateId} not found");

        if (request.TemplateVersion < 1 || request.TemplateVersion > template.CurrentVersion)
            throw new NotFoundException(
                $"Version {request.TemplateVersion} does not exist for template '{template.Name.Value}'. " +
                $"Valid versions: 1 to {template.CurrentVersion}");

        var templateVersion = template.Versions
            .FirstOrDefault(v => v.Version == request.TemplateVersion)
            ?? throw new NotFoundException(
                $"Version {request.TemplateVersion} not found for template '{template.Name.Value}'. " +
                $"Available versions: {string.Join(", ", template.Versions.Select(v => v.Version))}");

        var record = Record.Create(
            request.ArchiveId,
            request.FolderId,
            request.CustomerId,
            request.AppointmentId,
            request.TemplateId,
            templateVersion.Id,
            dateTimeProvider.UtcNow);

        foreach (var valueDto in request.Values)
        {
            var field = templateVersion.Fields
                .FirstOrDefault(f => f.Name.Value == valueDto.FieldName)
                ?? throw new NotFoundException($"Field '{valueDto.FieldName}' not found in template version {templateVersion.Version}");

            var value = new RecordFieldValue(field.Id, valueDto.FieldName, field.Type, JsonValueConverter.ToObject(valueDto.Value));
            record.AddValue(value);
        }

        recordRepository.Add(record);

        var response = new RecordResponse(
            record.Id,
            record.ArchiveId,
            record.FolderId,
            record.CustomerId,
            record.AppointmentId,
            record.TemplateId,
            record.TemplateVersionId,
            record.Values,
            record.Attachments,
            record.CreatedAt,
            record.UpdatedAt);

        return Result.Success(response);
    }
}
