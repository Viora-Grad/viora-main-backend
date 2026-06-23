using Microsoft.AspNetCore.Mvc;
using Viora.Domain.Organizations.LegalPapers.Internals;

namespace Viora.Api.Controllers.LegalPapers;

public record AddLegalPaperRequest(
    [FromForm] Guid ApplicationId,
    [FromForm] IFormFile File,
    [FromForm] LegalPaperType Type,
    [FromForm] string OfficialName,
    [FromForm] DateTime ExpiryDateUtc);

public record UpdateLegalPaperStatusRequest(AcceptanceStatus Status);
