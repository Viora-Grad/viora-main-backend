namespace Viora.Domain.Users.Identity;

public sealed class Permission
{
    public static readonly Permission UsersRead = new(1, "users:read");

    private Permission(int id, string name)
    {
        Id = id;
        Name = name;
    }
    public static Permission Create(int id, string name) => new(id, name);

    public int Id { get; init; }

    public string Name { get; init; }
}
