using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;

namespace Viora.Application.Archives.GetRecord;

internal class GetRecordQueryHandler(
    IRecordRepository recordRepository) : IQueryHandler<GetRecordQuery, RecordResponse>
{
    public async Task<Result<RecordResponse>> Handle(GetRecordQuery request, CancellationToken cancellationToken)
    {
        var record = await recordRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Record with id {request.Id} not found");

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
