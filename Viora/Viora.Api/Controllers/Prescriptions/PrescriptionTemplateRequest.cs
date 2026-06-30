using Microsoft.AspNetCore.Mvc;

namespace Viora.Api.Controllers.Prescriptions;

public class PrescriptionTemplateRequest
{
    [FromForm] public Guid OrganizationId { get; set; }
    [FromForm] public string Name { get; set; }
    [FromForm] public IFormFile File { get; set; }
    [FromForm] public double TopMargin { get; set; }
    [FromForm] public double RightMargin { get; set; }
    [FromForm] public double LeftMargin { get; set; }
    [FromForm] public double BottomMargin { get; set; }
}
