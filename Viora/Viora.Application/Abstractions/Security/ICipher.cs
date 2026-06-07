namespace Viora.Application.Abstractions.Security;

public interface ICipher
{
    public string Encrypt(string plainText);
    public string Decrypt(string cipherText);
}
