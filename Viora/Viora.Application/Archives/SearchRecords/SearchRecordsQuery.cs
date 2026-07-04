using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;

namespace Viora.Application.Archives.SearchRecords;

public sealed record SearchRecordsQuery(
    Guid ArchiveId,
    string? SearchTerm,
    Guid? FolderId,
    DateTime? FromDate,
    DateTime? ToDate
) : IQuery<List<RecordResponse>>;
