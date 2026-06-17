using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Viora.Application.AiRag.Abstractions;
using Viora.Domain.AiRag.Chat;
using Viora.Domain.AiRag.Intent;
using Viora.Domain.AiRag.Prompts;

namespace Viora.Application.AiRag.Handlers;

public sealed class SpecialtyHandler : IIntentHandler
{
    public ChatIntent Handles => ChatIntent.SpecialtyRecommendation;

    private readonly ISpecialtyVectorStore _store;
    private readonly IChatCompletionService _chat;
    private readonly Kernel _kernel;

    public SpecialtyHandler(ISpecialtyVectorStore store, IChatCompletionService chat, Kernel kernel)
    {
        _store = store;
        _chat = chat;
        _kernel = kernel;
    }

    public async Task<ChatResponse> HandleAsync(string message, DetectedIntent detected, ChatHistory history)
    {
        var specialties = await _store.SearchAsync(message, topK: 5);

        if (specialties.Count == 0)
        {
            return new ChatResponse
            {
                Message = "I wasn't able to determine a specialty for your concern. Please describe your symptoms in more detail.",
                Intent = ChatIntent.SpecialtyRecommendation,
            };
        }

        var ragHistory = new ChatHistory();
        ragHistory.AddSystemMessage(SpecialtyRagPrompt.Build(specialties));
        ragHistory.AddUserMessage(message);

        var result = await _chat.GetChatMessageContentAsync(ragHistory, kernel: _kernel);

        return new ChatResponse
        {
            Message = result.Content?.Trim() ?? "I'm sorry, I couldn't generate a response.",
            Intent = ChatIntent.SpecialtyRecommendation,
        };
    }
}