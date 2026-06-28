using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Shared.Internal;
using Viora.Domain.Staffs.Internal;
using Viora.Domain.Users.Identity;

namespace Viora.Domain.Staffs;

public sealed class Staff : Entity
{
    private readonly List<Role> _roles = new();
    private readonly List<Branch> _branches = new();
    public Guid OrganizationId { get; private set; }
    public FirstName FirstName { get; private set; } = null!;
    public LastName LastName { get; private set; } = null!;
    public UserName UserName { get; private set; } = null!;
    public HashedPassword HashedPassword { get; private set; } = null!;
    public DateOnly DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public StaffStatus StaffStatus { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; } = null!;

    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();
    public IReadOnlyCollection<Branch> Branches => _branches.AsReadOnly();
    private Staff() { } // For EF Core
    public Staff(
        Guid id,
        Guid organizationId,
        FirstName firstName,
        LastName lastName,
        UserName userName,
        HashedPassword hashedPassword,
        StaffStatus staffStatus,
        PhoneNumber phoneNumber) : base(id)
    {
        OrganizationId = organizationId;
        FirstName = firstName;
        LastName = lastName;
        UserName = userName;
        HashedPassword = hashedPassword;
        StaffStatus = staffStatus;
        PhoneNumber = phoneNumber;
    }

}
