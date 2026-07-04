using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;

namespace Viora.Application.Archives.UpdateRecord;

public sealed record UpdateRecordCommand(
    Guid Id,
    List<RecordFieldValueDto> Values
) : ICommand;
