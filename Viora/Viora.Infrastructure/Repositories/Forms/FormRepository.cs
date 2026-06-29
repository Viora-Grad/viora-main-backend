using Microsoft.EntityFrameworkCore;
using Viora.Domain.Forms;

namespace Viora.Infrastructure.Repositories.Forms;

internal class FormRepository : Repository<Form>, IFormRepository
{
    public FormRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<Form?> GetServiceFormAsync(Guid serviceId, CancellationToken cancellationToken)
    {
        return DbContext.Set<Form>()
         .FirstOrDefaultAsync(
             f => f.ServiceId == serviceId,
             cancellationToken);
    }
}
