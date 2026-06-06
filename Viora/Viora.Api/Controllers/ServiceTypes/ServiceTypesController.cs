using Microsoft.AspNetCore.Mvc;
using Viora.Domain.Shared;

namespace Viora.Api.Controllers.ServiceTypes;

[Route("api/[controller]")]
[ApiController]
public class ServiceTypesController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
        => Ok(ServiceType.All.Select(s => s.Value));
}
