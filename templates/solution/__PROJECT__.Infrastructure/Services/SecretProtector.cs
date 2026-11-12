using System.Security.Cryptography;
using System.Text;

namespace __PROJECT__.Infrastructure.Services;

internal static class SecretProtector
{
    private const string Prefix = "enc:v1:";

    public static string Protect(string value, string? passphrase)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(passphrase))
        {
            return value;
        }

        using var aes = Aes.Create();
        aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(passphrase));
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plain = Encoding.UTF8.GetBytes(value);
        var cipher = encryptor.TransformFinalBlock(plain, 0, plain.Length);
        var payload = new byte[aes.IV.Length + cipher.Length];
        Buffer.BlockCopy(aes.IV, 0, payload, 0, aes.IV.Length);
        Buffer.BlockCopy(cipher, 0, payload, aes.IV.Length, cipher.Length);
        return Prefix + Convert.ToBase64String(payload);
    }

    public static string? Unprotect(string? value, string? passphrase)
    {
        if (string.IsNullOrEmpty(value) || !value.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return value;
        }

        if (string.IsNullOrWhiteSpace(passphrase))
        {
            return null;
        }

        try
        {
            var payload = Convert.FromBase64String(value[Prefix.Length..]);
            using var aes = Aes.Create();
            aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(passphrase));
            var ivLength = aes.BlockSize / 8;
            var iv = payload[..ivLength];
            var cipher = payload[ivLength..];
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
            return Encoding.UTF8.GetString(plain);
        }
        catch
        {
            return null;
        }
    }
}
