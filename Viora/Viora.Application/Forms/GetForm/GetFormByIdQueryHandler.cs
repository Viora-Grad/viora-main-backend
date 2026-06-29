using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Forms.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Forms;

namespace Viora.Application.Forms.GetForm;

public class GetFormByIdQueryHandler(
    IFormRepository formRepository) : IQueryHandler<GetFormByIdQuery, FormResponse>
{
    public async Task<Result<FormResponse>> Handle(GetFormByIdQuery request, CancellationToken cancellationToken)
    {
        var form = await formRepository.GetByIdAsync(request.FormId, cancellationToken)
            ?? throw new NotFoundException($"the form with id {request.FormId} not found");
        var formResponse = new FormResponse(form.Id, form.StaffId, form.ServiceId, form.Name.value, form.Fields);
        return Result.Success(formResponse);
    }
}
