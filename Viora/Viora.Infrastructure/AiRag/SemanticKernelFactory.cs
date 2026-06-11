using Microsoft.SemanticKernel;

namespace Viora.Infrastructure.AiRag;

public class SemanticKernelFactory
{
    public static Kernel Create(GeminiSettings settings)
    {
        var builder = Kernel.CreateBuilder();

        // Gemini chat completion — used by handlers via IChatCompletionService
        builder.AddGoogleAIGeminiChatCompletion(
            modelId: settings.ChatModel,
            apiKey:  settings.ApiKey);

        // Gemini text embeddings — used by vector stores via ITextEmbeddingGenerationService
        builder.AddGoogleAIEmbeddingGenerator(
            modelId: settings.EmbeddingModel,
            apiKey:  settings.ApiKey);

        return builder.Build();
    }
}