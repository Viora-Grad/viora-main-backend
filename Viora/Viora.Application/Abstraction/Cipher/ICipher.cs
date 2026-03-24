namespace Viora.Application.Abstraction.Cipher;

public interface ICipher
{
    public string Encrypt(string plainText);
    public string Decrypt(string cipherText);
}
