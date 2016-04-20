using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Extensions;

namespace Warden.Web.Core.Services
{
    public interface IEncrypter
    {
        string GetSalt(string data);
        string GetHash(string data, string salt);
        string Encrypt(string text, string salt);
        string Decrypt(string text, string salt);
    }

    public class Encrypter : IEncrypter
    {
        private readonly string _key;
        private const int DeriveBytesIterationsCount = 10000;
        private const int MinSaltSize = 10;
        private const int MaxSaltSize = 20;

        public Encrypter(string key)
        {
            if (key.Empty())
                throw new DomainException("Encrypter key can not be empty.");

            _key = key;
        }

        public string GetSalt(string data)
        {
            var random = new Random();
            var saltSize = random.Next(MinSaltSize, MaxSaltSize);
            var saltBytes = new byte[saltSize];
            var rng = new RNGCryptoServiceProvider();
            rng.GetNonZeroBytes(saltBytes);

            return Convert.ToBase64String(saltBytes);
        }

        public string GetHash(string data, string salt)
        {
            using (var sha512 = SHA512.Create())
            {
                var bytes = Encoding.Unicode.GetBytes(data + salt);
                var hash = sha512.ComputeHash(bytes);

                return Convert.ToBase64String(hash);
            }
        }

        public string Encrypt(string text, string salt)
            => Encrypt<AesManaged>(text, _key, salt);

        public string Decrypt(string text, string salt)
            => Decrypt<AesManaged>(text, _key, salt);

        private static string Encrypt<T>(string text, string passwordKey, string salt) where T : SymmetricAlgorithm, new()
        {
            var transform = GetCryptoTransform<T>(passwordKey, salt);
            var buffer = new MemoryStream();
            using (var writer = new StreamWriter(new CryptoStream(buffer, transform, CryptoStreamMode.Write), Encoding.Unicode))
            {
                writer.Write(text);
            }

            return Convert.ToBase64String(buffer.ToArray());
        }

        private static string Decrypt<T>(string text, string passwordKey, string salt) where T : SymmetricAlgorithm, new()
        {
            var transform = GetCryptoTransform<T>(passwordKey, salt);
            var buffer = new MemoryStream(Convert.FromBase64String(text));
            using (var reader = new StreamReader(new CryptoStream(buffer, transform, CryptoStreamMode.Read), Encoding.Unicode))
            {
                return reader.ReadToEnd();
            }
        }

        private static ICryptoTransform GetCryptoTransform<T>(string passwordKey, string salt)
            where T : SymmetricAlgorithm, new()
        {
            var rgb = new Rfc2898DeriveBytes(passwordKey, Convert.FromBase64String(salt), DeriveBytesIterationsCount);
            var algorithm = new T();
            var rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            var rgbIv = rgb.GetBytes(algorithm.BlockSize >> 3);

            return algorithm.CreateEncryptor(rgbKey, rgbIv);
        }
    }
}