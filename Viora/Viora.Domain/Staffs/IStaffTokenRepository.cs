namespace Viora.Domain.Staffs;

public interface IStaffTokenRepository
{
    void Add(StaffToken staffToken);
    Task<StaffToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<StaffToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);
    void Remove(StaffToken staffToken);
}
