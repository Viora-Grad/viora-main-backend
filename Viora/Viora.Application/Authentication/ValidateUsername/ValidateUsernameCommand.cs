using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Authentication.ValidateUsername;

public sealed record ValidateUsernameCommand(Guid OrganizationId, string Username) : ICommand;
