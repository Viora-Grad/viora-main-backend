using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;

namespace Viora.Application.Archives.GetRecordsByFolder;

internal class GetRecordsByFolderQueryHandler(
    IRecordRepository recordRepository) : IQueryHandler<GetRecordsByFolderQuery, List<RecordResponse>>
{
    public async Task<Result<List<RecordResponse>>> Handle(GetRecordsByFolderQuery request, CancellationToken cancellationToken)
    {
        var records = await recordRepository.GetByFolderIdAsync(request.FolderId, cancellationToken);

        var response = records.Select(r => new RecordResponse(
            r.Id,
            r.ArchiveId,
            r.FolderId,
            r.CustomerId,
            r.AppointmentId,
            r.TemplateId,
            r.TemplateVersionId,
            r.Values,
            r.Attachments,
            r.CreatedAt,
            r.UpdatedAt
        )).ToList();

        return Result.Success(response);
    }
}
