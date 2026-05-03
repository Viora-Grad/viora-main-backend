namespace Viora.Application.Abstractions.Security;

public interface IPasswordHasher
{
    string HashPassword(string password);
    /// <summary>
    /// <remark>The return type of this method might differ if Asp.Net Identity is used</remark>
    /// </summary>
    /// <returns>Boolean indicating password matching</returns>
    bool VerifyPassword(string providedPassword, string hashedPassword);
}
