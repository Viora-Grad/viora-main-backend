using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;

namespace Viora.Api.Controllers.Vivi;

[Route("api/ai/[controller]")]
[ApiController]
public class ChatsController : ControllerBase
{
    [HttpGet("client/stream")]
    public IActionResult ClientStream()
    {
        HttpContext.TransformToStream();

        throw new NotImplementedException();
    }

    [HttpGet("health/stream")]
    public IActionResult HealthStream()
    {
        HttpContext.TransformToStream();

        throw new NotImplementedException();
    }

    // IGNORED for now
    [HttpGet("marketing/stream")]
    public IActionResult MarketingStream()
    {
        HttpContext.TransformToStream();

        throw new NotImplementedException();
    }
}
