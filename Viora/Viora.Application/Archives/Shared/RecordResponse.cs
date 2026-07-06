using Viora.Domain.Archives;
using Viora.Domain.Archives.Internals;

namespace Viora.Application.Archives.Shared;

public sealed record RecordResponse(
    Guid Id,
    Guid ArchiveId,
    Guid FolderId,
    Guid CustomerId,
    Guid? AppointmentId,
    Guid TemplateId,
    Guid TemplateVersionId,
    IReadOnlyCollection<RecordFieldValue> Values,
    IReadOnlyCollection<Attachment> Attachments,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
