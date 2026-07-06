using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Marketing.PublishPost;

// Flips a previously archived post live on Facebook. No quota is consumed here.
public sealed record PublishPostCommand(Guid ChatId) : ICommand;
