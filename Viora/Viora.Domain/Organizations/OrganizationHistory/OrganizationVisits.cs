using Viora.Domain.Abstractions;
using Viora.Domain.Users;

namespace Viora.Domain.Organizations.OrganizationHistory;

/// <summary>
/// A join entity that represents the many-to-many relationship between Organization and Customer,
/// capturing the history of visits to organizations by customers.
/// 
/// </summary>
public sealed class OrganizationVisits : Entity
{
    public Guid OrganizationId { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTime VisitedAt { get; private set; }

    public Organization Organization { get; private set; } = null!; // navigation property for ef core 
    public Customer Customer { get; private set; } = null!; // navigation property for ef core
    private OrganizationVisits() { } // for ef core
    private OrganizationVisits(Guid id, Guid organizationId, Guid customerId)
        : base(id)
    {
        OrganizationId = organizationId;
        CustomerId = customerId;
        VisitedAt = DateTime.UtcNow;
    }
    public static OrganizationVisits Create(Guid organizationId, Guid customerId)
    {
        return new OrganizationVisits(Guid.NewGuid(), organizationId, customerId);
    }
}

// This class is a placeholder for the Organization entity, will be merged later.
public class Organization : Entity
{
    private Organization() { } // for ef core

}