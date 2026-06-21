using Microsoft.SemanticKernel;

namespace Viora.Infrastructure.AiRag;

public class SemanticKernelFactory
{
    public static Kernel Build(GroqSettings settings)
    {
        var builder = Kernel.CreateBuilder();

        builder.AddOpenAIChatCompletion(
            modelId: settings.ChatModel,
            endpoint: new Uri("https://api.groq.com/openai/v1"),
            apiKey: settings.ApiKey);

        return builder.Build();
    }
}