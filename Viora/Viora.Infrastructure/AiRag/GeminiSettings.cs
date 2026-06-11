namespace Viora.Infrastructure.AiRag;

public class GeminiSettings
{
    public string ApiKey { get; set; } = Environment.GetEnvironmentVariable("GEMINI_API_KEY_RAG")!;

    /// Model used for chat completion (handlers, orchestrator)
    public string ChatModel { get; set; } = Environment.GetEnvironmentVariable("GEMINI_MODEL")!;

    /// Model used for intent classification (cheap, fast)
    public string ClassificationModel { get; set; } = Environment.GetEnvironmentVariable("GEMINI_MODEL")!;

    /// Model used to generate embeddings for Qdrant indexing and search
    public string EmbeddingModel { get; set; } = Environment.GetEnvironmentVariable("EMBEDDING_MODEL")!;

    public int MaxTokens { get; set; } = 2048;
    public float Temperature { get; set; } = 0.3f;
}