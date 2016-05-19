using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Linq;
using NLog;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Dto;
using Warden.Web.Core.Extensions;
using Warden.Web.Core.Factories;
using Warden.Web.Core.Mongo.Queries;
using Warden.Web.Core.Settings;

namespace Warden.Web.Core.Services
{
    public interface IUserService
    {
        Task<UserDto> GetAsync(string email);
        Task<UserDto> GetAsync(Guid id);
        Task<bool> IsActiveAsync(Guid id);
        Task RegisterAsync(string email, string password, Role role = Role.User);

        Task LoginAsync(string email, string password, string ipAddress,
            string userAgent = null, string referrer = null);

        Task SetRecentlyViewedWardenInOrganizationAsync(Guid userId, Guid organizationId, Guid wardenId);
        Task SetNewPasswordAsync(Guid userId, string actualPassword, string newPassword);
        Task InitiatePasswordResetAsync(string email, string ipAddress = null, string userAgent = null);
        Task ValidatePasswordResetTokenAsync(string email, string token);

        Task CompletePasswordResetAsync(string email, string token, string password,
            string ipAddress = null, string userAgent = null);
    }

    public class UserService : IUserService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IMongoDatabase _database;
        private readonly IEncrypter _encrypter;
        private readonly IEmailSender _emailSender;
        private readonly ISecuredOperationFactory _securedOperationFactory;
        private readonly AccountSettings _accountSettings;

        public UserService(IMongoDatabase database, IEncrypter encrypter, 
            IEmailSender emailSender, ISecuredOperationFactory securedOperationFactory,
            AccountSettings accountSettings)
        {
            _database = database;
            _encrypter = encrypter;
            _emailSender = emailSender;
            _securedOperationFactory = securedOperationFactory;
            _accountSettings = accountSettings;
        }

        public async Task<UserDto> GetAsync(string email)
        {
            var user = await _database.Users().GetByEmailAsync(email);

            return await GetUserWithApiKeysAsync(user);
        }

        public async Task<UserDto> GetAsync(Guid id)
        {
            var user = await _database.Users().GetByIdAsync(id);

            return await GetUserWithApiKeysAsync(user);
        }

        public async Task<bool> IsActiveAsync(Guid id) => await _database.Users().IsActiveAsync(id);

        private async Task<UserDto> GetUserWithApiKeysAsync(User user)
        {
            if (user == null)
                return null;

            var apiKeys = await _database.ApiKeys().GetAllForUserAsync(user.Id);

            return new UserDto(user)
            {
                ApiKeys = apiKeys.Select(x => x.Key)
            };
        }

        public async Task RegisterAsync(string email, string password, Role role = Role.User)
        {
            if(!email.IsEmail())
                throw new ServiceException($"Invalid email: '{email}'.");
            if (password.Empty())
                throw new ServiceException("Password can not be empty.");

            var userExists = await _database.Users().ExistsAsync(email);
            if(userExists)
                throw new ServiceException($"User with email: '{email}' is already registered.");

            var user = new User(email, password, _encrypter, role);
            if(_accountSettings.AutoActivation)
                user.Activate();

            await _database.Users().InsertOneAsync(user);
            Logger.Info($"New user account: '{email}' was created with id: '{user.Id}'");
            await _emailSender.SendAccountCreatedEmailAsync(email);
        }

        public async Task LoginAsync(string email, string password, string ipAddress,
            string userAgent = null, string referrer = null)
        {
            if (!email.IsEmail())
                throw new ServiceException($"Invalid email: '{email}.");
            if (password.Empty())
                throw new ServiceException("Password can not be empty.");

            var user = await _database.Users().GetByEmailAsync(email);
            if (user == null)
                throw new ServiceException($"User has not been found for email: '{email}'.");
            if (user.State == State.Inactive)
                throw new ServiceException("User account has not been activated.");
            if (user.State == State.Locked)
                throw new ServiceException("User account has been locked.");
            if (user.State == State.Deleted)
                throw new ServiceException("User account has been deleted.");

            if (!user.ValidatePassword(password, _encrypter))
                throw new ServiceException("Invalid credentials.");

            var session = new UserSession(user, userAgent, ipAddress, referrer);
            await _database.UserSessions().InsertOneAsync(session);
            Logger.Info($"New user session: '{session.Id}' was created " +
                        $"for user: '{user.Email}' with id: '{user.Id}'.");
        }

        public async Task SetRecentlyViewedWardenInOrganizationAsync(Guid userId, Guid organizationId, Guid wardenId)
        {
            var user = await _database.Users().GetByIdAsync(userId);
            var organization = await _database.Organizations().GetByIdAsync(organizationId);
            user.SetRecentlyViewedWardenInOrganization(organization, wardenId);
            await _database.Users().ReplaceOneAsync(x => x.Id == userId, user);
        }

        public async Task SetNewPasswordAsync(Guid userId, string actualPassword, string newPassword)
        {
            var user = await _database.Users().GetByIdAsync(userId);
            if (user == null)
                throw new ServiceException($"User has not been found for id: '{userId}'.");

            if (!user.ValidatePassword(actualPassword, _encrypter))
                throw new ServiceException("Invalid actual password.");

            user.SetPassword(newPassword, _encrypter);
            await _database.Users().ReplaceOneAsync(x => x.Id == userId, user);
            Logger.Info($"User: '{user.Email}' with: '{user.Id}' has changed password.");
        }

        public async Task InitiatePasswordResetAsync(string email, string ipAddress = null, string userAgent = null)
        {
            var user = await _database.Users().GetByEmailAsync(email);
            if (user == null)
                throw new ServiceException($"User has not been found for email: '{email}'.");

            var securedOperation = _securedOperationFactory.Create(SecuredOperationType.ResetPassword,
                DateTime.UtcNow.AddDays(1), user.Id, email, ipAddress, userAgent);

            await _database.SecuredOperations().InsertOneAsync(securedOperation);
            await _emailSender.SendResetPasswordEmailAsync(email, securedOperation.Token);
            Logger.Info($"User '{user.Email}' [id: '{user.Id}'] has initiated password reset.");
        }

        public async Task ValidatePasswordResetTokenAsync(string email, string token)
        {
            var user = await _database.Users().GetByEmailAsync(email);
            if (user == null)
                throw new ServiceException($"User has not been found for email: '{email}'.");

            var securedOperation = await _database.SecuredOperations()
                .GetByUserIdAndTokenAsync(user.Id, token);
            if (securedOperation == null)
                throw new ServiceException("Invalid token.");

            securedOperation.CheckIfCanBeConsumedOrFail();
        }

        public async Task CompletePasswordResetAsync(string email, string token, string password, string ipAddress = null,
            string userAgent = null)
        {
            var user = await _database.Users().GetByEmailAsync(email);
            if (user == null)
                throw new ServiceException($"User has not been found for email: '{email}'.");

            var securedOperation = await _database.SecuredOperations()
                .GetByUserIdAndTokenAsync(user.Id, token);

            if (securedOperation == null)
                throw new ServiceException("Invalid token.");

            user.SetPassword(password, _encrypter);
            securedOperation.Consume(ipAddress, userAgent);

            await _database.Users().ReplaceOneAsync(x => x.Id == user.Id, user);
            await _database.SecuredOperations().ReplaceOneAsync(x => x.Id == securedOperation.Id, securedOperation);
            await _emailSender.SendPasswordChangedEmailAsync(email);

            Logger.Info($"User '{user.Email}' [id: '{user.Id}'] has completed password reset.");
        }
    }
}