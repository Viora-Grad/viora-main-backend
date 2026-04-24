namespace Viora.Domain.Users.Identity;

public sealed class Role
{
    public static readonly Role None = new(0, "None");
    public static readonly Role Registered = new(1, "Registered");

    public Role(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; init; }

    public string Name { get; init; }

    public ICollection<AuthAccount> Users { get; init; } = new List<AuthAccount>();

    public ICollection<Permission> Permissions { get; init; } = new List<Permission>();
}
