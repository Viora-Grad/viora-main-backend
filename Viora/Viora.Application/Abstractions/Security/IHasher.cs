namespace Viora.Application.Abstractions.Security;

public interface IHasher
{
    string Hash(string input);
    /// <summary>
    /// <remark>The return type of this method might differ if Asp.Net Identity is used</remark>
    /// </summary>
    /// <returns>Boolean indicating password matching</returns>
    bool Verify(string provided, string hash);
}
