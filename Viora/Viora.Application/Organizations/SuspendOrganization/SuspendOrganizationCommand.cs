using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Organizations.SuspendOrganization;

public record SuspendOrganizationCommand(Guid OrganizationId, Guid? SuspendedById, string Reason, string Notes) : ICommand;
