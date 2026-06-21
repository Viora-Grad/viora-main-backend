namespace Viora.Application.AiRag.Abstractions;

public interface IKnowledgeChunkMapper
{
    /// <summary>
    /// Splits the given markdown content into knowledge chunks.
    /// </summary>
    IReadOnlyList<KnowledgeChunk> FromMarkdown(string markdownContent);
}