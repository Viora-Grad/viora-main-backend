namespace Viora.Domain.Users.Identity;

public sealed class Role
{
    public static readonly Role None = new(0, "None");
    public static readonly Role Registered = new(1, "Registered");
    public static readonly Role Owner = new(2, "Owner");
    public static readonly Role Admin = new(3, "Admin");
    public static readonly Role Customer = new(4, "Customer");

    public Role(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; init; }

    public string Name { get; init; }

    public ICollection<User> Users { get; init; } = new List<User>();

    public ICollection<Permission> Permissions { get; init; } = new List<Permission>();
}
