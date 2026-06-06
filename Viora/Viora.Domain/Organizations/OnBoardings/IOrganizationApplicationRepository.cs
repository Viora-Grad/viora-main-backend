using Viora.Domain.Abstractions;

namespace Viora.Domain.Organizations.OnBoardings;

public interface IOrganizationApplicationRepository
{
    public void Add(OrganizationApplication application);
    public Task<OrganizationApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<OrganizationApplication>> ListAsync(ISpecification<OrganizationApplication> spec, CancellationToken cancellationToken = default);
    public Task<long> CountAsync(ISpecification<OrganizationApplication> spec, CancellationToken cancellationToken = default);
    public Task<bool> IsApplicationSubmittedForOwnerAsync(Guid id, CancellationToken cancellation = default);
    public Task<OrganizationApplication?> GetLatestApplicationForOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default);
    public Task<OrganizationApplication?> GetActiveApplicationByOrganizationNameAsync(string proposedName, CancellationToken cancellationToken);
}
