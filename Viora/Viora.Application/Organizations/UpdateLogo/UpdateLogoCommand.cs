namespace Viora.Application.Organizations.UpdateLogo;

public record UpdateLogoCommand(Guid OrganizationId, Guid MediaId);