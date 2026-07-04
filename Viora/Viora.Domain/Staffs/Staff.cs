using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Services;
using Viora.Domain.Shared.Internal;
using Viora.Domain.Staffs.Internal;
using Viora.Domain.Users.Identity;

namespace Viora.Domain.Staffs;

public sealed class Staff : Entity
{
    private readonly List<Role> _roles = new();
    private readonly List<Branch> _branches = new();
    private readonly List<Service> _services = new();
    public Guid OrganizationId { get; private set; }
    public FirstName? FirstName { get; private set; }
    public LastName? LastName { get; private set; }
    public Username? Username { get; private set; }
    public HashedPassword? HashedPassword { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public Gender? Gender { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public StaffStatus StaffStatus { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public bool IsDeleted => DeletedAt.HasValue;

    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();
    public IReadOnlyCollection<Branch> Branches => _branches.AsReadOnly();
    public IReadOnlyCollection<Service> Services => _services.AsReadOnly();
    private Staff() { } // For EF Core
    private Staff(Guid id, Guid organizationId, DateTime createdAt) : base(id)
    {
        OrganizationId = organizationId;
        CreatedAt = createdAt;
        StaffStatus = StaffStatus.Pending;
    }
    public static Staff Create(Guid organizationId, DateTime createdAt, Guid? id = null) // id is made an argumnet mainly for seeding purposes,
    {
        return new Staff(id ?? Guid.NewGuid(), organizationId, createdAt);
    }
    public void AddRoles(IEnumerable<Role> roles)
    {
        if (roles is null || !roles.Any())
            throw new ArgumentException("Roles cannot be null or empty.");
        foreach (var role in roles)
        {
            if (!_roles.Contains(role))
                _roles.Add(role);
        }
    }
    public void AssignBranches(IEnumerable<Branch> branches)
    {
        if (branches is null || !branches.Any())
            throw new ArgumentException("Branches cannot be null or empty.");
        foreach (var branch in branches)
        {
            if (!_branches.Contains(branch))
                _branches.Add(branch);
        }
    }
    public void AssignServices(IEnumerable<Service> services)
    {
        if (services is null || !services.Any())
            throw new ArgumentException("Services cannot be null or empty.");
        foreach (var service in services)
        {
            if (!_services.Contains(service))
                _services.Add(service);
        }
    }
    public void SetStaffProperties(
        FirstName firstName,
        LastName lastName,
        Username username,
        HashedPassword hashedPassword,
        DateOnly dateOfBirth,
        Gender gender,
        PhoneNumber phoneNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        Username = username;
        HashedPassword = hashedPassword;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        PhoneNumber = phoneNumber;
    }
    public Result Activate()
    {
        if (StaffStatus == StaffStatus.Active)
            return Result.Failure(StaffErrors.StaffAlreadyActive);


        if (!IsValidStaffInstance())
            return Result.Failure(StaffErrors.InvalidStaffInstance);



        StaffStatus = StaffStatus.Active;
        return Result.Success();
    }
    private bool IsValidStaffInstance()
    {
        return FirstName is not null &&
               LastName is not null &&
               Username is not null &&
               HashedPassword is not null &&
               DateOfBirth is not null &&
               Gender is not null &&
               PhoneNumber is not null &&
               _branches.Count != 0;
    }
    public Result Suspend()
    {
        if (StaffStatus == StaffStatus.Suspended)
            return Result.Failure(StaffErrors.StaffAlreadySuspended);
        StaffStatus = StaffStatus.Suspended;
        return Result.Success();
    }
    public Result Delete(DateTime deletedAt)
    {
        DeletedAt = deletedAt;
        return Result.Success();
    }
    public void RemoveRoles(IEnumerable<Role> roles)
    {
        if (roles is null || !roles.Any())
            throw new ArgumentException("Roles cannot be null or empty.");
        foreach (var role in roles)
        {
            if (_roles.Contains(role))
                _roles.Remove(role);
        }
    }
    public static Staff SeedActiveStaff(
        Guid Id,
        Guid organizationId,
        string firstName,
        string lastName,
        DateTime createdAt,
        DateOnly? dateOfBirth,
        Gender gender,
        PhoneNumber phoneNumber)
    {
        var staff = new Staff(Id, organizationId, createdAt)
        {
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth ?? new DateOnly(2000, 1, 1),
            Gender = gender,
            PhoneNumber = phoneNumber
        };
        return staff;
    }
}
