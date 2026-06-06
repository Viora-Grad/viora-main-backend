using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Organizations.HideOrganization;

public record HideOrganizationCommand(Guid OrganizationId) : ICommand;