using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Organizations.UpdateLogo;

public record UpdateLogoCommand(Guid OrganizationId, Guid MediaId) : ICommand;