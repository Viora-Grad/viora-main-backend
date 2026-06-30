using Microsoft.SemanticKernel.ChatCompletion;
using Viora.Domain.AiRag.Chat;
using Viora.Domain.AiRag.Intent;

namespace Viora.Application.AiRag.Handlers;

public class GreetingHandler : IIntentHandler
{
    public ChatIntent Handles => ChatIntent.Greeting;

    public Task<ChatResponse> HandleAsync(string message, DetectedIntent detected, ChatHistory history) =>
        Task.FromResult(new ChatResponse
            {
                Message = "Hello! I'm Vivi 👋\n\n" +
                          "I can help you with:\n" +
                          "• Finding the right specialist based on your symptoms\n" +
                          "• Searching for healthcare organizations by name, country, or service type\n" +
                          "• Answering questions about Viora's features\n\n" +
                          "What can I help you with today?",
            }
        );
}