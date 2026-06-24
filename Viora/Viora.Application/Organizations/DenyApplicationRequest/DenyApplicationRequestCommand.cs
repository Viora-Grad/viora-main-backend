using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Organizations.DenyApplicationRequest;

public sealed record DenyApplicationRequestCommand(Guid ApplicationId, Guid RejectedById) : ICommand;