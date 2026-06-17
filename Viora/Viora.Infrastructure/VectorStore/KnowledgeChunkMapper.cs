using System.Security.Cryptography;
using System.Text;
using Viora.Application.AiRag.Abstractions;

namespace Viora.Infrastructure.VectorStore;

public sealed class KnowledgeChunkMapper : IKnowledgeChunkMapper
{
    public IReadOnlyList<KnowledgeChunk> FromMarkdown(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown)) return [];

        var result  = new List<KnowledgeChunk>();
        var heading = "Introduction";
        var body    = new StringBuilder();

        foreach (var line in markdown.Split('\n'))
        {
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                if (body.Length > 0)
                    result.Add(Build(heading, body.ToString()));

                heading = line[3..].Trim();
                body.Clear();
            }
            else
            {
                body.AppendLine(line);
            }
        }

        if (body.Length > 0)
            result.Add(Build(heading, body.ToString()));

        return result;
    }

    private static KnowledgeChunk Build(string heading, string body)
    {
        var content = $"## {heading}\n\n{body.Trim()}";
        var hash    = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return new KnowledgeChunk(new Guid(hash[..16]), content, heading);
    }
}