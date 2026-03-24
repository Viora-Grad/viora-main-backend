using System.Security.Cryptography;
using System.Text;
using Viora.Application.Abstraction.Cipher;

namespace Viora.Infrastructure.Cipher;


public class Cipher : ICipher
{
    private static readonly byte[] Key = new byte[32];
    private static readonly int IvLength = 16;

    public Cipher()
    {
        string keyString = Environment.GetEnvironmentVariable("CipherPrivateKey")!;
        byte[] keyBytes = Encoding.UTF8.GetBytes(keyString);
        Array.Copy(keyBytes, Key, Math.Min(keyBytes.Length, Key.Length));
    }

    public string Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.GenerateIV();

            byte[] encryptedBytes;
            using (var encryptor = aes.CreateEncryptor())
            using (var ms = new MemoryStream())
            {
                // embeding the IV at the begging for single string storage

                ms.Write(aes.IV, 0, IvLength);
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }
                encryptedBytes = ms.ToArray();
            }

            return Convert.ToBase64String(encryptedBytes);
        }
    }

    public string Decrypt(string cipherText)
    {
        byte[] encryptedBytes = Convert.FromBase64String(cipherText);
        if (encryptedBytes.Length < IvLength)
            throw new ArgumentException("Invalid ciphertext was detected while decrypting");

        byte[] iv = new byte[IvLength];
        byte[] cipherBytes = new byte[encryptedBytes.Length - IvLength];
        Array.Copy(encryptedBytes, 0, iv, 0, IvLength);
        Array.Copy(encryptedBytes, IvLength, cipherBytes, 0, cipherBytes.Length);

        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = iv;

            using (var decrypter = aes.CreateDecryptor())
            using (var ms = new MemoryStream(cipherBytes))
            using (var cs = new CryptoStream(ms, decrypter, CryptoStreamMode.Read))
            using (var sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
