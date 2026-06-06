using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Vivi.Shared.Internals;

namespace Viora.Application.Vivi.SendMessage;

public sealed record SendMessageCommand(Guid? SessionId, Guid? UserId, Persona Persona, string Message) : ICommand<SendMessageResponse>;
