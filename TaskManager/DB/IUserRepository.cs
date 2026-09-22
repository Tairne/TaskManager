using TaskManager.DB.DataModels;
using TaskManager.DB.DTO;
using Task = System.Threading.Tasks.Task;

namespace TaskManager.DB
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetUserAsync(string login, string password, CancellationToken cancellationToken);
        Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken);
        Task<List<AllUsersDto>> GetAllUserListAsync(CancellationToken cancellationToken);
        Task SaveRefreshToken(User user, string token, CancellationToken cancellationToken);
    }
}
