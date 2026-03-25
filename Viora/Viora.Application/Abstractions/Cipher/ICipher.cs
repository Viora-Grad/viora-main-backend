namespace Viora.Application.Abstractions.Cipher;

public interface ICipher
{
    public string Encrypt(string plainText);
    public string Decrypt(string cipherText);
}
