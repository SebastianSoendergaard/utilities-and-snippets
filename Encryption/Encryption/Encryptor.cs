using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Encryption
{
    public static class Encryptor
    {
        public static string Encrypt<TData>(string key, TData input) where TData : class
        {
            var json = JsonSerializer.Serialize(input);
            return Encrypt(key, json);
        }

        public static string Encrypt(string key, string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            var keyBytes = new Rfc2898DeriveBytes(key, 20, 1000, HashAlgorithmName.SHA512);

            using var aes = Aes.Create();
            aes.Key = keyBytes.GetBytes(32);
            aes.IV = keyBytes.GetBytes(16);

            using var memoryStream = new MemoryStream();
            using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cryptoStream.Write(inputBytes, 0, inputBytes.Length);
            }

            var encryptedBytes = memoryStream.ToArray();

            return $"{Convert.ToBase64String(keyBytes.Salt)}.{Convert.ToBase64String(encryptedBytes)}";
        }

        public static TData? Decrypt<TData>(string key, string input)
        {
            var json = Decrypt(key, input);
            return JsonSerializer.Deserialize<TData>(json);
        }

        public static string Decrypt(string key, string input)
        {
            var encryptionParts = input.Split('.');
            var salt = Convert.FromBase64String(encryptionParts[0]);
            var encryptedBytes = Convert.FromBase64String(encryptionParts[1]);

            var keyBytes = new Rfc2898DeriveBytes(key, salt, 1000, HashAlgorithmName.SHA512);

            using var aes = Aes.Create();
            aes.Key = keyBytes.GetBytes(32);
            aes.IV = keyBytes.GetBytes(16);

            using var inputMemoryStream = new MemoryStream(encryptedBytes);
            using var outputMemoryStream = new MemoryStream();
            using (var cryptoStream = new CryptoStream(inputMemoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
            {
                cryptoStream.CopyTo(outputMemoryStream);
            }

            return System.Text.Encoding.UTF8.GetString(outputMemoryStream.ToArray());
        }
    }
}
