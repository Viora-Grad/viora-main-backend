using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.Payments.CreatePaymentSession;

namespace Viora.Api.Controllers.Payments;

[ApiController]
public class PaymentController : ControllerBase
{
    private readonly ISender _sender;

    public PaymentController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Route("api/payments/session/{orderId:guid}")]
    [Authorize(Roles = "Customer,Owner")]
    public async Task<IActionResult> CreateSession(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreatePaymentSessionCommand(orderId), cancellationToken);
        return result.ToActionResult();
    }
}
