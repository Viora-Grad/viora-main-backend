using Microsoft.EntityFrameworkCore;
using Viora.Domain.Forms;

namespace Viora.Infrastructure.Repositories.Forms;

internal class FormSubmissionMediaRepository : Repository<FormSubmissionMedia>, IFormSubmissionMediaRepository
{
    public FormSubmissionMediaRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<List<FormSubmissionMedia>> GetByFormSubmissionIdAsync(Guid formSubmissionId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<FormSubmissionMedia>()
            .Where(fm => fm.FormSubmissionId == formSubmissionId)
            .ToListAsync();
    }
}
