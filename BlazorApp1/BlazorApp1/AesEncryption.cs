using System.Security.Cryptography;
using System.Text;

namespace BlazorApp1
{
    public class AesEncryption
    {
        private static byte[] _key = Encoding.UTF8.GetBytes("0123456789abcdef"); 
        private static byte[] _iv = Encoding.UTF8.GetBytes("abcdef0123456789"); 

        public static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                using var sw = new StreamWriter(cs);
                sw.Write(plainText);
            }

            var encryptedBytes = ms.ToArray();
            return Convert.ToBase64String(encryptedBytes);
        }
        public static string Decrypt(string cipherText)
        {
            var cipherBytes = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var ms = new MemoryStream(cipherBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}
