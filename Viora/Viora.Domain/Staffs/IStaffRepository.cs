namespace Viora.Domain.Staffs;

public interface IStaffRepository
{
    public Task<Staff?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
