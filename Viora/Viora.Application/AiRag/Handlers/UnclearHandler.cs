using Microsoft.SemanticKernel.ChatCompletion;
using Viora.Domain.AiRag.Chat;
using Viora.Domain.AiRag.Intent;

namespace Viora.Application.AiRag.Handlers;

public class UnclearHandler : IIntentHandler
{
    public ChatIntent Handles => ChatIntent.Unclear;

    public Task<ChatResponse> HandleAsync(string message, DetectedIntent detected, ChatHistory history)
    {
        var response = new ChatResponse
        {
            Message = "I'm sorry, I didn't quite understand that. Could you please rephrase or provide more details?",
            Intent = ChatIntent.Unclear,
        };

        return Task.FromResult(response);
    }
}
