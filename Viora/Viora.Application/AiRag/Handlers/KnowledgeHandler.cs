using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Viora.Application.AiRag.Abstractions;
using Viora.Domain.AiRag.Chat;
using Viora.Domain.AiRag.Intent;
using Viora.Domain.AiRag.Prompts;

namespace Viora.Application.AiRag.Handlers;

public sealed class KnowledgeHandler : IIntentHandler
{
    public ChatIntent Handles => ChatIntent.KnowledgeQuery;

    private readonly IKnowledgeVectorStore _store;
    private readonly IChatCompletionService _chat;
    private readonly Kernel _kernel;

    public KnowledgeHandler(IKnowledgeVectorStore store, IChatCompletionService chat, Kernel kernel)
    {
        _store = store;
        _chat = chat;
        _kernel = kernel;
    }

    public async Task<ChatResponse> HandleAsync(string message, DetectedIntent detected, ChatHistory history)
    {
        var chunks = await _store.SearchAsync(message, topK: 3);

        if (chunks.Count == 0)
        {
            return new ChatResponse
            {
                Message = "I don't have information about that. Please contact Viora support.",
                Intent = ChatIntent.KnowledgeQuery,
            };
        }

        var ragHistory = new ChatHistory();
        ragHistory.AddSystemMessage(KnowledgeRagPrompt.Build(chunks));
        ragHistory.AddUserMessage(message);

        var result = await _chat.GetChatMessageContentAsync(ragHistory, kernel: _kernel);

        return new ChatResponse
        {
            Message = result.Content?.Trim() ?? "I'm sorry, I couldn't generate a response.",
            Intent = ChatIntent.KnowledgeQuery,
        };
    }
}