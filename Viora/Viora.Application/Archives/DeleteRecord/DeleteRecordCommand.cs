using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Archives.DeleteRecord;

public sealed record DeleteRecordCommand(Guid Id) : ICommand;
