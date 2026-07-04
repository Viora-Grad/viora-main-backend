using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;

namespace Viora.Application.Archives.GetRecord;

public sealed record GetRecordQuery(Guid Id) : IQuery<RecordResponse>;
