using TaskManager.DB.DataModels;
using TaskManager.DB.DTO;

namespace TaskManager.Services.Interfaces
{
    public interface IAccountService
    {
        Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<User> CreateAccountAsync(CreateUserDto accountData, CancellationToken cancellationToken);
        Task<bool> UpdateAccountAsync(int id, UpdateUserDto accountData, CancellationToken cancellationToken);
        Task<List<AllUsersDto>> GetAllAsync(CancellationToken cancellationToken);
        Task<bool> ChangePasswordAsync(int id, ChangePasswordDto password, CancellationToken cancellationToken);
    }
}
