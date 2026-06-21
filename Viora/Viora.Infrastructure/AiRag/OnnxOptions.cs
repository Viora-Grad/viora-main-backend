namespace Viora.Infrastructure.AiRag;

public class OnnxOptions
{
    public string ModelPath { get; set; } = "Models/all-MiniLM-L6-v2/model.onnx";
    public string VocabPath { get; set; } = "Models/all-MiniLM-L6-v2/vocab.txt";
    public int MaxLength { get; set; } = 256;
}
