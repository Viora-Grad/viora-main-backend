using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Viora.Domain.MedicalInquiries;

namespace Viora.Application.AiRag.Ingestion;

/// <summary>
/// Streams specialty inquiries from a JSON array without loading the whole
/// document into memory. Each item gets a deterministic id derived from its
/// category + question, so re-ingesting overwrites rather than duplicating.
/// </summary>
public static class SpecialtyInquiryParser
{
    public static async IAsyncEnumerable<MedicalInquiry> ParseAsync(
        Stream json,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var items = JsonSerializer.DeserializeAsyncEnumerable<MedicalInquiryJson>(
            json, cancellationToken: ct);

        await foreach (var item in items.WithCancellation(ct))
        {
            if (item is null) continue;

            var id = new Guid(SHA256.HashData(
                Encoding.UTF8.GetBytes($"{item.Category}:{item.Question}"))[..16]);

            yield return new MedicalInquiry
            {
                Id = id.ToString(),
                Question = item.Question,
                Specialty = item.Category,
            };
        }
    }
}
