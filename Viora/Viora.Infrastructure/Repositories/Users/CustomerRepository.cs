using Microsoft.EntityFrameworkCore;
using Viora.Domain.Users.Customers;

namespace Viora.Infrastructure.Repositories.Users;

internal class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public override Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return DbContext.Set<Customer>()
            .Include(customer => customer.MedicalRecord)
            .FirstOrDefaultAsync(customer => customer.Id == id, cancellationToken);
    }



}
