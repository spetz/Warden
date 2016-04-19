using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Warden.Web.Core.Domain;

namespace Warden.Web.Core.Services
{
    public interface IEncrypter
    {
        byte[] GetSalt(string data);
        byte[] GetHash(string data, byte[] salt);
        string Encrypt(string text, byte[] salt);
        string Decrypt(string text, byte[] salt);
    }

    public class Encrypter : IEncrypter
    {
        private readonly string _key;
        private const int DeriveBytesIterationsCount = 10000;
        private const int MinSaltSize = 8;
        private const int MaxSaltSize = 12;

        public Encrypter(string key)
        {
            if (key.Empty())
                throw new DomainException("Encrypter key can not be empty.");

            _key = key;
        }

        public byte[] GetSalt(string data)
        {
            var random = new Random();
            var saltSize = random.Next(MinSaltSize, MaxSaltSize);
            var saltBytes = new byte[saltSize];
            var rng = new RNGCryptoServiceProvider();
            rng.GetNonZeroBytes(saltBytes);

            return saltBytes;
        }

        public byte[] GetHash(string data, byte[] salt)
        {
            using (var sha512 = SHA512.Create())
            {
                var bytes = Encoding.Unicode.GetBytes(data + salt);
                var hash = sha512.ComputeHash(bytes);

                return hash;
            }
        }

        public string Encrypt(string text, byte[] salt)
            => Encrypt<AesManaged>(text, _key, salt);

        public string Decrypt(string text, byte[] salt)
            => Decrypt<AesManaged>(text, _key, salt);

        private static string Encrypt<T>(string text, string passwordKey, byte[] salt) where T : SymmetricAlgorithm, new()
        {
            var rgb = new Rfc2898DeriveBytes(passwordKey, salt, DeriveBytesIterationsCount);
            var algorithm = new T();
            var rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            var rgbIv = rgb.GetBytes(algorithm.BlockSize >> 3);
            var transform = algorithm.CreateEncryptor(rgbKey, rgbIv);
            var buffer = new MemoryStream();
            using (var writer = new StreamWriter(new CryptoStream(buffer, transform, CryptoStreamMode.Write), Encoding.Unicode))
            {
                writer.Write(text);
            }

            return Convert.ToBase64String(buffer.ToArray());
        }

        private static string Decrypt<T>(string text, string passwordKey, byte[] salt) where T : SymmetricAlgorithm, new()
        {
            var rgb = new Rfc2898DeriveBytes(passwordKey, salt, DeriveBytesIterationsCount);
            var algorithm = new T();
            var rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            var rgbIv = rgb.GetBytes(algorithm.BlockSize >> 3);
            var transform = algorithm.CreateDecryptor(rgbKey, rgbIv);
            var buffer = new MemoryStream(Convert.FromBase64String(text));
            using (var reader = new StreamReader(new CryptoStream(buffer, transform, CryptoStreamMode.Read), Encoding.Unicode))
            {
                return reader.ReadToEnd();
            }
        }
    }
}