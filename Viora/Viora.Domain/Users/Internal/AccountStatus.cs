namespace Viora.Domain.Users.Internal;

public enum AccountStatus
{
    Active,
    Suspended,
    Deleted, // soft delete implementation
    NotActive, // if an account has not been used for a while (not sure if this is helpful but just in case)
    Deactivated,
}
