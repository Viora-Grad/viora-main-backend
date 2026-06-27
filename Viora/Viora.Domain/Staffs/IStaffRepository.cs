namespace Viora.Domain.Staffs;

public interface IStaffRepository
{
    void Add(Staff staff);
    Task<Staff?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
