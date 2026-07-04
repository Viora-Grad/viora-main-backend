using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;

namespace Viora.Application.Archives.DeleteRecord;

internal class DeleteRecordCommandHandler(
    IRecordRepository recordRepository) : ICommandHandler<DeleteRecordCommand>
{
    public async Task<Result> Handle(DeleteRecordCommand request, CancellationToken cancellationToken)
    {
        var record = await recordRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Record with id {request.Id} not found");

        recordRepository.Remove(record);
        return Result.Success();
    }
}
