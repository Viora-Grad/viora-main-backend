using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viora.Infrastructure.AiRag;

public class GroqSettings
{
    public string ApiKey { get; set; } = Environment.GetEnvironmentVariable("GROQ_API_KEY")!;

    public string ChatModel { get; set; } = Environment.GetEnvironmentVariable("GROQ_MODEL") ?? "llama-3.1-8b-instant";

    public string ClassificationModel { get; set; } = Environment.GetEnvironmentVariable("GROQ_MODEL") ?? "llama-3.1-8b-instant";

    public int MaxTokens { get; set; } = 2048;
    public float Temperature { get; set; } = 0.3f;
}
