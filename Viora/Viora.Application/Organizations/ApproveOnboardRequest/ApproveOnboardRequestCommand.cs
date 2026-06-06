using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Organizations.ApproveOnboardRequest;

public record ApproveOnboardRequestCommand(Guid RequestId) : ICommand<Guid>;
