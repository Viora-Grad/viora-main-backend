namespace Viora.Application.Abstractions.Security;

public interface IHasher
{
    string Hash(string input);
    bool Verify(string provided, string hash);
}
