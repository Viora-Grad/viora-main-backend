using Viora.Domain.Abstractions;

namespace Viora.Domain.Users;
/// <summary>
/// <strong>Resolving User instances to Customer in first assignment
/// might allow Customer and Owner existence for the same UserId which might not be a bad resolution</strong>
/// </summary>

public sealed class Customer : Entity
{
    private readonly HashSet<Guid> _organizationsVisited = [];
    public Guid UserId { get; private set; } // required and Unique
    public Guid? MedicalRecordId { get; private set; } // can be removed since the relation is optional from the customer side
    public IReadOnlyList<Guid> OrganizationsVisited => _organizationsVisited.ToList().AsReadOnly();
    private Customer() { } // for ef core
    private Customer(Guid id, Guid userId, Guid? medicalRecordId)
        : base(id)
    {
        UserId = userId;
        MedicalRecordId = medicalRecordId;
    }
    public static Customer Create(Guid userId, Guid? medicalRecordId)
    {
        // add any validation if needed
        return new Customer(Guid.NewGuid(), userId, medicalRecordId);
    }
    public Result AddMedicalRecord(Guid medicalRecordId)
    {
        // might trigger domain events for medical record change
        MedicalRecordId = medicalRecordId;
        return Result.Success();
    }

    public Result VisitOrganization(Guid organizationId)
    {
        _organizationsVisited.Add(organizationId);
        return Result.Success();
    }
    public Result RemoveVisitedOrganization(Guid organizationId)
    {
        _organizationsVisited.Remove(organizationId);
        return Result.Success();
    }
    public Result<bool> IsOrganizationVisited(Guid organizationId)
    {
        return Result.Success(_organizationsVisited.Contains(organizationId));
    }

}
