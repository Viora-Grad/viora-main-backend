using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

namespace Viora.Infrastructure.AiRag;

public sealed class OnnxEmbeddingService : ITextEmbeddingGenerationService, IDisposable
{
    private readonly InferenceSession _session;
    private readonly BertTokenizer _tokenizer;
    private readonly int _maxLength;
    private readonly int _dimension;

    public OnnxEmbeddingService(OnnxOptions options)
    {
        // Tune for CPU throughput: full graph optimization and intra-op
        // parallelism across physical cores. Set once at session creation.
        var sessionOptions = new SessionOptions
        {
            GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
            IntraOpNumThreads = Environment.ProcessorCount,
            EnableMemoryPattern = true,
        };

        _session = new InferenceSession(options.ModelPath, sessionOptions);
        _tokenizer = new BertTokenizer(options.VocabPath);
        _maxLength = options.MaxLength;

        var outputMeta = _session.OutputMetadata[_session.OutputNames[0]];
        _dimension = (int)outputMeta.Dimensions[^1];
    }

    public IReadOnlyDictionary<string, object?> Attributes =>
        new Dictionary<string, object?>();

    public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(
        IList<string> data,
        Kernel? kernel,
        CancellationToken cancellationToken = default)
    {
        if (data.Count == 0) return [];

        // Tokenize all texts and find the longest sequence in this batch
        var allIds = new long[data.Count][];
        var allMasks = new long[data.Count][];
        var allLengths = new int[data.Count];
        var batchLen = 0;

        for (int i = 0; i < data.Count; i++)
        {
            var (ids, mask, _) = _tokenizer.Encode(data[i], _maxLength);
            allIds[i] = ids;
            allMasks[i] = mask;
            var len = ids.AsSpan().LastIndexOfAnyExcept(0L) + 1;
            allLengths[i] = len;
            if (len > batchLen) batchLen = len;
        }

        // Build batched tensors — pad to batchLen within the batch
        var inputIds = new DenseTensor<long>([data.Count, batchLen]);
        var attentionMask = new DenseTensor<long>([data.Count, batchLen]);
        var tokenTypeIds = new DenseTensor<long>([data.Count, batchLen]);

        for (int i = 0; i < data.Count; i++)
        {
            var len = allLengths[i];
            for (int j = 0; j < batchLen; j++)
            {
                inputIds[i, j] = j < len ? allIds[i][j] : 0;
                attentionMask[i, j] = j < len ? allMasks[i][j] : 0;
                tokenTypeIds[i, j] = 0;
            }
        }

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask),
            NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds),
        };

        using var outputs = _session.Run(inputs);
        var hiddenState = outputs.First().AsTensor<float>();

        var results = new List<ReadOnlyMemory<float>>(data.Count);
        for (int i = 0; i < data.Count; i++)
            results.Add(MeanPool(hiddenState, i, allLengths[i]));

        return results;
    }

    private ReadOnlyMemory<float> MeanPool(Tensor<float> hiddenState, int batchIdx, int actualLen)
    {
        var result = new float[_dimension];
        float maskSum = 0;

        for (int j = 0; j < actualLen; j++)
        {
            maskSum++;
            for (int k = 0; k < _dimension; k++)
                result[k] += hiddenState[batchIdx, j, k];
        }

        if (maskSum > 0)
            for (int k = 0; k < _dimension; k++)
                result[k] /= maskSum;

        var norm = MathF.Sqrt(result.Sum(x => x * x));
        if (norm > 0)
            for (int k = 0; k < _dimension; k++)
                result[k] /= norm;

        return result;
    }

    public void Dispose() => _session?.Dispose();
}
