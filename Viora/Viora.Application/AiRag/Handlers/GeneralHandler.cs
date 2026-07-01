using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Viora.Application.AiRag.Abstractions;
using Viora.Domain.AiRag;
using Viora.Domain.AiRag.Chat;
using Viora.Domain.AiRag.Intent;
using Viora.Domain.AiRag.Prompts;

namespace Viora.Application.AiRag.Handlers;

public class GeneralHandler : IIntentHandler
{
    public ChatIntent Handles => ChatIntent.General;

    private readonly IChatCompletionService _chat;
    private readonly IKnowledgeVectorStore _store;
    private readonly Kernel _kernel;

    public GeneralHandler(IChatCompletionService chat, IKnowledgeVectorStore store, Kernel kernel)
    {
        _chat = chat;
        _store = store;
        _kernel = kernel;
    }

    private static readonly PromptExecutionSettings DefaultSettings = new()
    {
        ExtensionData = new Dictionary<string, object>
        {
            ["temperature"] = 0.7,
            ["max_tokens"] = 1024,
        },
    };

    public async Task<ChatResponse> HandleAsync(string message, DetectedIntent detected, ChatHistory history, UserContext? userContext = null)
    {
        // Build a snapshot of the conversation so far, plus the system prompt,
        // without mutating the session's ChatHistory (the orchestrator handles AppendUser/AppendAssistant).
        var llmHistory = new ChatHistory();

        // Retrieve relevant knowledge base chunks to ground the response
        var chunks = await _store.SearchAsync(message, topK: 3);
        if (chunks.Count > 0)
        {
            llmHistory.AddSystemMessage(KnowledgeRagPrompt.Build(chunks));
        }
        else
        {
            llmHistory.AddSystemMessage(GeneralPrompt.Build());
        }

        foreach (var msg in history)
            llmHistory.Add(msg);
        llmHistory.AddUserMessage(message);

        var result = await _chat.GetChatMessageContentAsync(llmHistory, DefaultSettings, _kernel);

        return new ChatResponse
        {
            Message = result.Content?.Trim() ?? "I'm sorry, I couldn't generate a response.",
            Intent = ChatIntent.General,
        };
    }
}