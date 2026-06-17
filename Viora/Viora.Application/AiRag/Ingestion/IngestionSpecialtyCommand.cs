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

    public async Task ExecuteAsync(IEnumerable<MedicalInquiry> inquiries, CancellationToken ct = default)
    {
        if (inquiries == null)
            throw new ArgumentException("Inquiries cannot be null.", nameof(inquiries));

        await _store.IndexAsync(inquiries, ct);
    }
}