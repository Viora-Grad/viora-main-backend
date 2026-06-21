using Viora.Application.AiRag.Abstractions;
using Viora.Domain.MedicalInquiries;

namespace Viora.Application.AiRag.Ingestion;

public sealed class IngestSpecialtyCommand
{
    private readonly ISpecialtyVectorStore _store;

    public IngestSpecialtyCommand(ISpecialtyVectorStore store)
    {
        _store = store;
    }

    public async Task ExecuteAsync(IAsyncEnumerable<MedicalInquiry> inquiries, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(inquiries);

        await _store.IndexAsync(inquiries, ct);
    }
}