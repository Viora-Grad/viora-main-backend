using Microsoft.EntityFrameworkCore;
using Viora.Domain.Organizations.LegalPapers;

namespace Viora.Infrastructure.Repositories.Organizations;

internal class LegalPaperRepository(ApplicationDbContext dbContext) : Repository<LegalPaper>(dbContext), ILegalPaperRepository
{
    public async Task<IEnumerable<LegalPaper>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<LegalPaper>()
            .Where(x => x.ApplicationId == applicationId)
            .ToListAsync(cancellationToken);
    }
}
