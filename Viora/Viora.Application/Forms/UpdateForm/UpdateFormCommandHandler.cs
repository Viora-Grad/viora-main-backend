using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Forms;

namespace Viora.Application.Forms.UpdateForm;

public class UpdateFormCommandHandler(
    IFormRepository formRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateFormCommand>
{
    public async Task<Result> Handle(UpdateFormCommand request, CancellationToken cancellationToken)
    {
        var form = await formRepository.GetByIdAsync(request.FormId, cancellationToken)
            ?? throw new NotFoundException($"the form with id {request.FormId} not found");
        form.Update(request.newFields);
        await unitOfWork.SaveChangesAsync();
        return Result.Success();
    }
}
