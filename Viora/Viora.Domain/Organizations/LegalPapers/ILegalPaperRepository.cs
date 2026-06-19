namespace Viora.Domain.Organizations.LegalPapers;

public interface ILegalPaperRepository
{
    void Add(LegalPaper legalPaper);
    Task<LegalPaper?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<LegalPaper>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
}
