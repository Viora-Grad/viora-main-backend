namespace Viora.Domain.Users.Identity;

public sealed class Role
{
    public static readonly Role None = new(0, "None");
    public static readonly Role Registered = new(1, "Registered");
    public static readonly Role Owner = new(2, "Owner");
    public static readonly Role Admin = new(3, "Admin");
    public static readonly Role Customer = new(4, "Customer");

    public Role(int id, string name, string? description = null, Guid? tenantId = null)
    {
        Id = id;
        Name = name;
        Description = description;
        TenantId = tenantId;
    }
    public Role(string name, string? description = null, Guid? tenantId = null) // let EF Core handle the Id generation for new roles
    {
        Name = name;
        Description = description;
        TenantId = tenantId;

    }
    public int Id { get; init; }

    public string Name { get; init; }
    public string? Description { get; init; }
    public Guid? TenantId { get; init; } // which refers to the Organization if the role is specific to an organization, otherwise null for global roles
    public ICollection<User> Users { get; init; } = new List<User>();

    public ICollection<Permission> Permissions { get; init; } = new List<Permission>();

    public static IReadOnlyList<Role> All => [None, Registered, Owner, Admin, Customer];    // no need to reflect it unlike in service type (if more roles are to be added use reflection)
}
