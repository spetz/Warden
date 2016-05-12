using System;
using System.Linq;
using Warden.Web.Core.Domain;

namespace Warden.Web.Core.Factories
{
    public interface ISecuredOperationFactory
    {
        SecuredOperation Create(SecuredOperationType operationType, DateTime expiry,
            Guid? userId = null, string email = null,
            string ipAddress = null, string userAgent = null);
    }

    public class SecuredOperationFactory : ISecuredOperationFactory
    {
        private const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static readonly Random Random = new Random();

        public SecuredOperation Create(SecuredOperationType operationType, DateTime expiry,
            Guid? userId = null, string email = null,
            string ipAddress = null, string userAgent = null)
        {
            var token = new string(Enumerable.Repeat(Chars, Random.Next(80, 120))
                .Select(s => s[Random.Next(s.Length)]).ToArray());

            var operation = new SecuredOperation(operationType, token, expiry,
                userId, email, ipAddress, userAgent);

            return operation;
        }
    }
}