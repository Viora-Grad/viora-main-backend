using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Forms.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Forms;

namespace Viora.Application.Forms.GetServiceForm;

public class GetServiceFormQueryHandler(
    IFormRepository formRepository) : IQueryHandler<GetServiceFormQuery, FormResponse>
{
    public async Task<Result<FormResponse>> Handle(GetServiceFormQuery request, CancellationToken cancellationToken)
    {
        var form = await formRepository.GetServiceFormAsync(request.ServiceId, cancellationToken)
            ?? throw new NotFoundException($"the service with Id {request.ServiceId} does not have form ");

        var formResponse = new FormResponse(form.Id, form.StaffId, form.ServiceId, form.Name.value, form.Fields);
        return Result.Success(formResponse);
    }
}
