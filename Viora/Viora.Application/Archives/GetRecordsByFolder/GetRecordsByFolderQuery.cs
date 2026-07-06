using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;

namespace Viora.Application.Archives.GetRecordsByFolder;

public sealed record GetRecordsByFolderQuery(Guid FolderId) : IQuery<List<RecordResponse>>;
