using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;
using Viora.Domain.Archives.Internals;

namespace Viora.Application.Archives.UpdateRecord;

internal class UpdateRecordCommandHandler(
    IRecordRepository recordRepository,
    ITemplateRepository templateRepository) : ICommandHandler<UpdateRecordCommand>
{
    public async Task<Result> Handle(UpdateRecordCommand request, CancellationToken cancellationToken)
    {
        var record = await recordRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Record with id {request.Id} not found");

        var template = await templateRepository.GetByIdAsync(record.TemplateId, cancellationToken)
            ?? throw new NotFoundException($"Template with id {record.TemplateId} not found");

        var templateVersion = template.Versions
            .FirstOrDefault(v => v.Id == record.TemplateVersionId)
            ?? throw new NotFoundException($"Template version {record.TemplateVersionId} not found for template {record.TemplateId}");

        record.ClearValues();

        foreach (var valueDto in request.Values)
        {
            var field = templateVersion.Fields
                .FirstOrDefault(f => f.Name.Value == valueDto.FieldName)
                ?? throw new NotFoundException($"Field '{valueDto.FieldName}' not found in template version {record.TemplateVersionId}");

            var value = new RecordFieldValue(field.Id, valueDto.FieldName, field.Type, JsonValueConverter.ToObject(valueDto.Value));
            record.AddValue(value);
        }

        recordRepository.Update(record);
        return Result.Success();
    }
}
