using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Web.Services
{
    public class EncryptionService : IEncryption
    {

        private readonly string _encryptionKey = "sscYyr+k1EjnpNoZnil2S6o67zaRWAaEdGVzdF8wYzhlY";
        private readonly string _hashKey = "F8wYzhlYzdhZi1hOTIwLTQ5MWItODcyOC0yYzJhMzk2Z";

        public EncryptionService()
        {
            if (CommonUtility.GetAppConfigValue("EncryptionKey") != null)
            {
                _encryptionKey = CommonUtility.GetAppConfigValue("EncryptionKey");
            }
            if (CommonUtility.GetAppConfigValue("HashKey") != null)
            {
                _hashKey = CommonUtility.GetAppConfigValue("HashKey");
            }
        }

        public string GetHash(string inputString)
        {
            return GetHash(inputString, _hashKey);
        }

        public string GetHash(string inputString, string salt)
        {
            return Encrypt(inputString, string.Format("{0}{1}", _encryptionKey, salt));
        }

        public string Encrypt(string inputString)
        {
            return Encrypt(inputString, _encryptionKey);
        }

        public string Encrypt(string inputString, string encryptionKey)
        {
            if (string.IsNullOrEmpty(encryptionKey))
                throw new ArgumentException("Key must have valid value.", nameof(encryptionKey));
            if (string.IsNullOrEmpty(inputString))
                throw new ArgumentException("The text must have valid value.", nameof(inputString));

            var buffer = Encoding.UTF8.GetBytes(inputString);
            var hash = new SHA512CryptoServiceProvider();
            var aesKey = new byte[24];
            Buffer.BlockCopy(hash.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey)), 0, aesKey, 0, 24);

            using (var aes = Aes.Create())
            {
                if (aes == null)
                    throw new ArgumentException("Parameter must not be null.", nameof(aes));

                aes.Key = aesKey;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var resultStream = new MemoryStream())
                {
                    using (var aesStream = new CryptoStream(resultStream, encryptor, CryptoStreamMode.Write))
                    using (var plainStream = new MemoryStream(buffer))
                    {
                        plainStream.CopyTo(aesStream);
                    }

                    var result = resultStream.ToArray();
                    var combined = new byte[aes.IV.Length + result.Length];
                    Array.ConstrainedCopy(aes.IV, 0, combined, 0, aes.IV.Length);
                    Array.ConstrainedCopy(result, 0, combined, aes.IV.Length, result.Length);

                    return Convert.ToBase64String(combined);
                }
            }
        }

        public string Decrypt(string inputString)
        {
            return Decrypt(inputString, _encryptionKey);
        }

        public string Decrypt(string inputString, string encryptionKey)
        {
            if (string.IsNullOrEmpty(encryptionKey))
                throw new ArgumentException("Key must have valid value.", nameof(encryptionKey));
            if (string.IsNullOrEmpty(inputString))
                throw new ArgumentException("The encrypted text must have valid value.", nameof(inputString));

            var combined = Convert.FromBase64String(inputString);
            var buffer = new byte[combined.Length];
            var hash = new SHA512CryptoServiceProvider();
            var aesKey = new byte[24];
            Buffer.BlockCopy(hash.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey)), 0, aesKey, 0, 24);

            using (var aes = Aes.Create())
            {
                if (aes == null)
                    throw new ArgumentException("Parameter must not be null.", nameof(aes));

                aes.Key = aesKey;

                var iv = new byte[aes.IV.Length];
                var ciphertext = new byte[buffer.Length - iv.Length];

                Array.ConstrainedCopy(combined, 0, iv, 0, iv.Length);
                Array.ConstrainedCopy(combined, iv.Length, ciphertext, 0, ciphertext.Length);

                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var resultStream = new MemoryStream())
                {
                    using (var aesStream = new CryptoStream(resultStream, decryptor, CryptoStreamMode.Write))
                    using (var plainStream = new MemoryStream(ciphertext))
                    {
                        plainStream.CopyTo(aesStream);
                    }

                    return Encoding.UTF8.GetString(resultStream.ToArray());
                }
            }
        }

        public byte[] Encrypt(byte[] data, string encryptionKey)
        {
            TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
            byte[] trimmedBytes = new byte[24];
            var byteArr = Encoding.UTF8.GetBytes(encryptionKey);
            Buffer.BlockCopy(byteArr, 0, trimmedBytes, 0, 24);
            tripleDES.Key = trimmedBytes;
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tripleDES.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(data, 0, data.Length);
            tripleDES.Clear();

            return resultArray;
        }

        public byte[] Decrypt(byte[] data, string encryptionKey)
        {
            TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
            byte[] trimmedBytes = new byte[24];
            var byteArr = Encoding.UTF8.GetBytes(encryptionKey);
            Buffer.BlockCopy(byteArr, 0, trimmedBytes, 0, 24);
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tripleDES.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(data, 0, data.Length);
            tripleDES.Clear();
            return resultArray;
        }
    }
}
