using Microsoft.EntityFrameworkCore;
using Viora.Domain.Forms;

namespace Viora.Infrastructure.Repositories.Forms;

internal class FormSubmissionRepository : Repository<FormSubmission>, IFormSubmissionRepository
{
    public FormSubmissionRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<FormSubmission?> GetByAppointmentIdAsync(Guid AppointmentId, Guid FormId, CancellationToken cancellationToken)
    {
        return DbContext.Set<FormSubmission>()
            .FirstOrDefaultAsync(fs => fs.AppointmentId == AppointmentId && fs.FormId == FormId);
    }
}
