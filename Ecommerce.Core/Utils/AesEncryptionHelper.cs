namespace Ecommerce.Core.Utils;
using System.Security.Cryptography;
using System.Text;

public static class AesEncryptionHelper
{
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("cD7wQ2FmSx9nZpRkVb8uT5sLxE1aR4qZ");
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("A1b2C3d4E5f6G7h8");

    public static string EncryptString(string plainText)
    {
        using Aes? aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;
        using ICryptoTransform? encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using MemoryStream ms = new MemoryStream();
        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (StreamWriter sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }
        // URL-safe base64
        return Convert.ToBase64String(ms.ToArray()).Replace('+', '-').Replace('/', '_').Replace("=", "");
    }

    public static string DecryptString(string cipherText)
    {
        // Restore padding for base64
        string b64 = cipherText.Replace('-', '+').Replace('_', '/');
        switch (b64.Length % 4)
        {
            case 2: b64 += "=="; break;
            case 3: b64 += "="; break;
        }

        byte[] buffer = Convert.FromBase64String(b64);

        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;
        using ICryptoTransform? decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using MemoryStream ms = new MemoryStream(buffer);
        using CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using StreamReader sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}
