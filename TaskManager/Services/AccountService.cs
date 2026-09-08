using FluentValidation;
using TaskManager.DB;
using TaskManager.DB.DataModels;
using TaskManager.DB.DTO;
using TaskManager.Services.Interfaces;
using TaskManager.Validators;

namespace TaskManager.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserRepository repository;
        private readonly IPasswordService passwordService;

        public AccountService(UserRepository repository, IPasswordService passwordService)
        {
            this.repository = repository;
            this.passwordService = passwordService;
        }

        public async Task<User> CreateAccountAsync(CreateUserDto accountData, CancellationToken cancellationToken)
        {
            var existingUser = await repository.GetByLoginAsync(
                accountData.Login,
                cancellationToken);

            if (existingUser != null)
                throw new DuplicateLoginException(accountData.Login);

            var validator = new CreateUserValidator();
            var result = await validator.ValidateAsync(accountData, cancellationToken);

            if (!result.IsValid)
                throw new ValidationException(result.Errors);

            User user = new User
            {
                Login = accountData.Login,
                Password = passwordService.Hash(accountData.Password),
                UserName = accountData.UserName,
                Position = accountData.Position
            };

            return await repository.AddAsync(user, cancellationToken);
        }

        public async Task<List<AllUsersDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await repository.GetAllUserListAsync(cancellationToken);
        }

        public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await repository.GetAsync(id, cancellationToken);
        }
            
        public async Task<bool> UpdateAccountAsync(int id, UpdateUserDto accountData, CancellationToken cancellationToken)
        {
            var user = await repository.GetAsync(id, cancellationToken);

            if (user == null)
            {
                return false;
            }

            var validator = new UpdateUserValidator();
            var result = await validator.ValidateAsync(accountData, cancellationToken);

            if (!result.IsValid)
                throw new ValidationException(result.Errors);

            user.Login = accountData.Login;
            user.UserName = accountData.UserName;
            user.Position = accountData.Position;

            await repository.UpdateAsync(user, cancellationToken);
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int id, ChangePasswordDto password, CancellationToken cancellationToken)
        {
            var user = await repository.GetAsync(id, cancellationToken);

            if (user == null)
            {
                throw new KeyNotFoundException($"User id {id} not found");
            }

            var validator = new ChangePasswordValidator();
            var result = await validator.ValidateAsync(password, cancellationToken);

            if (!result.IsValid)
                throw new ValidationException(result.Errors);

            if (!passwordService.Verify(password.CurrentPassword, user.Password))
            {
                return false;
            }

            user.Password = passwordService.Hash(password.NewPassword);
            await repository.UpdateAsync(user, cancellationToken);
            return true;
        }
    }
}
