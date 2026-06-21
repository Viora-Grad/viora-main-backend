// This is a simplified BERT tokenizer implementation in C#. It supports basic tokenization and WordPiece tokenization.
// It loads the vocabulary from a file and provides an Encode method to convert text into input IDs
// suitable for BERT models. The tokenizer handles special tokens like [CLS], [SEP], and [PAD], and can be used

using System.Text.RegularExpressions;

namespace Viora.Infrastructure.AiRag;

public sealed class BertTokenizer
{
    private readonly Dictionary<string, int> _vocab;
    private readonly int _unkId;
    private readonly int _clsId;
    private readonly int _sepId;
    private readonly int _padId;

    public BertTokenizer(string vocabPath)
    {
        var lines = File.ReadAllLines(vocabPath);
        _vocab = new Dictionary<string, int>(lines.Length);

        for (int i = 0; i < lines.Length; i++)
            _vocab[lines[i].Trim()] = i;

        _unkId = _vocab.GetValueOrDefault("[UNK]", 100);
        _clsId = _vocab.GetValueOrDefault("[CLS]", 101);
        _sepId = _vocab.GetValueOrDefault("[SEP]", 102);
        _padId = _vocab.GetValueOrDefault("[PAD]", 0);
    }

    public int VocabSize => _vocab.Count;

    public (long[] InputIds, long[] AttentionMask, long[] TokenTypeIds) Encode(
        string text, int maxLength = 256)
    {
        var tokens = new List<int> { _clsId };

        foreach (var word in BasicTokenize(text))
        {
            if (tokens.Count >= maxLength - 1) break;
            tokens.AddRange(WordPieceToIds(word));
        }

        tokens.Add(_sepId);

        if (tokens.Count > maxLength)
            tokens = tokens.Take(maxLength).ToList();

        var inputIds = new long[maxLength];
        var attentionMask = new long[maxLength];

        for (int i = 0; i < tokens.Count; i++)
        {
            inputIds[i] = tokens[i];
            attentionMask[i] = 1;
        }

        for (int i = tokens.Count; i < maxLength; i++)
            inputIds[i] = _padId;

        return (inputIds, attentionMask, new long[maxLength]);
    }

    private static IEnumerable<string> BasicTokenize(string text)
    {
        text = text.ToLowerInvariant();
        text = Regex.Replace(text, @"([\p{P}\p{S}])", " $1 ");
        text = Regex.Replace(text, @"\s+", " ").Trim();
        return text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    private List<int> WordPieceToIds(string word)
    {
        if (_vocab.TryGetValue(word, out var id))
            return [id];

        var ids = new List<int>();
        var remaining = word.AsSpan();

        while (remaining.Length > 0)
        {
            var found = false;

            for (int len = remaining.Length; len > 0; len--)
            {
                var subword = ids.Count == 0
                    ? remaining[..len].ToString()
                    : "##" + remaining[..len].ToString();

                if (_vocab.TryGetValue(subword, out var subId))
                {
                    ids.Add(subId);
                    remaining = remaining[len..];
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                ids.Add(_unkId);
                break;
            }
        }

        return ids;
    }
}
