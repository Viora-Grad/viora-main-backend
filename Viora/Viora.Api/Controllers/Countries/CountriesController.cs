using Microsoft.AspNetCore.Mvc;
using Viora.Domain.Shared;

namespace Viora.Api.Controllers.Countries;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class CountriesController(IReadOnlyList<Country> countries) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CountryResponse>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(countries.Select(c => new CountryResponse(c.Id, c.Name, c.IsoAlphaThree, c.Nationality)));
    }
}

public record CountryResponse(Guid Id, string Name, string IsoAlphaThree, string Nationality);
