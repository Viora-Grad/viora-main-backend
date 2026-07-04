using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;

namespace Viora.Application.Archives.CreateRecord;

public sealed record CreateRecordCommand(
    Guid ArchiveId,
    Guid FolderId,
    Guid CustomerId,
    Guid? AppointmentId,
    Guid TemplateId,
    int TemplateVersion,
    List<RecordFieldValueDto> Values
) : ICommand<RecordResponse>;
