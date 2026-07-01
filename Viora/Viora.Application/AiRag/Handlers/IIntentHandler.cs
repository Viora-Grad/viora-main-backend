using Microsoft.SemanticKernel.ChatCompletion;
using Viora.Domain.AiRag;
using Viora.Domain.AiRag.Chat;
using Viora.Domain.AiRag.Intent;

namespace Viora.Application.AiRag.Handlers;

public interface IIntentHandler
{
    ChatIntent Handles { get; }
    
    /// history contains all prior turns + any retrieved context injected by the handler.
    Task<ChatResponse> HandleAsync(string message, DetectedIntent detected, ChatHistory history, UserContext? userContext = null);
}