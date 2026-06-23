namespace Viora.Domain.Staff;

public interface IStaffRepository
{
    Task<Staff?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
