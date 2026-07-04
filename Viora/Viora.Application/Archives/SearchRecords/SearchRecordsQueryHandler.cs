using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;

namespace Viora.Application.Archives.SearchRecords;

internal class SearchRecordsQueryHandler(
    IRecordRepository recordRepository) : IQueryHandler<SearchRecordsQuery, List<RecordResponse>>
{
    public async Task<Result<List<RecordResponse>>> Handle(SearchRecordsQuery request, CancellationToken cancellationToken)
    {
        var records = await recordRepository.SearchAsync(
            request.ArchiveId,
            request.SearchTerm,
            request.FolderId,
            request.FromDate,
            request.ToDate,
            cancellationToken);

        var response = records.Select(r => new RecordResponse(
            r.Id,
            r.ArchiveId,
            r.FolderId,
            r.CustomerId,
            r.AppointmentId,
            r.TemplateId,
            r.TemplateVersionId,
            r.Values.Where(v => v.FieldName.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)).ToList(),
            r.Attachments,
            r.CreatedAt,
            r.UpdatedAt
        )).ToList();

        return Result.Success(response);
    }
}
