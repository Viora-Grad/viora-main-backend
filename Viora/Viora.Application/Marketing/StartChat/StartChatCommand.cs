using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Marketing.StartChat;

// Starts a new marketing chat (== a new post draft). An optional first prompt is processed immediately.
public sealed record StartChatCommand(string? FirstMessage) : ICommand<StartChatResponse>;
