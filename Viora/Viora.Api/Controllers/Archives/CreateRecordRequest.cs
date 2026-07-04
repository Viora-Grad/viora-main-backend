using Viora.Application.Archives.Shared;

namespace Viora.Api.Controllers.Archives;

public sealed record CreateRecordRequest(
    Guid FolderId,
    Guid CustomerId,
    Guid? AppointmentId,
    Guid TemplateId,
    int TemplateVersion,
    List<RecordFieldValueDto> Values
);
