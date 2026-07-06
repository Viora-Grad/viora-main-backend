using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.Billings.GetOrganizationInvoices;

namespace Viora.Api.Controllers.Billings;

[ApiController]
public class InvoiceController : ControllerBase
{
    private readonly ISender _sender;

    public InvoiceController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Route("api/organization/{organizationId:guid}/invoices")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<IActionResult> GetOrganizationInvoices(Guid organizationId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetOrganizationInvoicesQuery(organizationId), cancellationToken);
        return result.ToActionResult();
    }
}
