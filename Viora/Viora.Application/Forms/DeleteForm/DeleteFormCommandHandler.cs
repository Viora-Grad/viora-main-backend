using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Forms;

namespace Viora.Application.Forms.DeleteForm;

public class DeleteFormCommandHandler(
    IFormRepository formRepository,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<DeleteFormCommand>
{
    public async Task<Result> Handle(DeleteFormCommand request, CancellationToken cancellationToken)
    {
        var form = await formRepository.GetByIdAsync(request.FormId, cancellationToken)
            ?? throw new NotFoundException($"the form with id {request.FormId} not found");

        formRepository.Remove(request.FormId);
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
