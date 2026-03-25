using Microsoft.EntityFrameworkCore;
using Viora.Domain.Abstractions;


namespace Viora.Infrastructure;

internal class ApplicationDbContext : DbContext, IUnitOfWork
{
    public override Task<int> SaveChangesAsync(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}