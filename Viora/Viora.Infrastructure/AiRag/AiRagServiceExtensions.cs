using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using Viora.Application.AiRag.Abstractions;
using Viora.Application.AiRag.Handlers;
using Viora.Application.AiRag.Ingestion;
using Viora.Application.AiRag.Intent;
using Viora.Application.AiRag.Orchestrator;
using Viora.Application.AiRag.Session;
using Viora.Domain.ChatSessions;
using Viora.Infrastructure.Repositories;
using Viora.Infrastructure.VectorStore;

namespace Viora.Infrastructure.AiRag;

public static class AiRagServiceExtensions
{
    public static IServiceCollection AddAiRagServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var groq = configuration.GetSection("AiRag:Groq").Get<GroqSettings>()
            ?? throw new InvalidOperationException("AiRag:Groq config section is missing.");

        services.AddSingleton(_ => new QdrantClient(
            host: configuration["AiRag:Qdrant:Host"] ?? "localhost",
            port: configuration.GetValue("AiRag:Qdrant:Port", 6334)));

        var onnxOpts = configuration.GetSection("AiRag:Onnx").Get<OnnxOptions>()
            ?? new OnnxOptions();
        services.AddSingleton<ITextEmbeddingGenerationService>(
            _ => new OnnxEmbeddingService(onnxOpts));

        services.AddScoped<Kernel>(_ => SemanticKernelFactory.Build(groq));

        var bootstrap = SemanticKernelFactory.Build(groq);
        services.AddSingleton(bootstrap.GetRequiredService<IChatCompletionService>());

        services.AddSingleton<IKnowledgeVectorStore, KnowledgeVectorStore>();
        services.AddSingleton<ISpecialtyVectorStore, SpecialtyVectorStore>();

        services.AddScoped<IKnowledgeChunkMapper, KnowledgeChunkMapper>();

        services.AddScoped<IChatSessionRepository, ChatSessionRepository>();

        services.AddScoped<ChatSessionService>();
        services.AddScoped<SessionFlushService>();
        services.AddScoped<LoadSessionCommand>();
        services.AddScoped<IntentDetectionService>();
        services.AddScoped<IngestKnowledgeCommand>();
        services.AddScoped<IngestSpecialtyCommand>();
        services.AddScoped<GetSessionHistoryQuery>();
        services.AddScoped<AiOrchestratorService>();

        services.AddScoped<IIntentHandler, GreetingHandler>();
        services.AddScoped<IIntentHandler, GeneralHandler>();
        services.AddScoped<IIntentHandler, UnclearHandler>();
        services.AddScoped<IIntentHandler, KnowledgeHandler>();
        services.AddScoped<IIntentHandler, SpecialtyHandler>();
        services.AddScoped<IIntentHandler, OrganizationSearchHandler>();

        return services;
    }
}