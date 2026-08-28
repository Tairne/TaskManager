using Microsoft.EntityFrameworkCore;
using TaskManager.DB.DataModels;
using TaskManager.DB.DTO;

namespace TaskManager.DB
{
    public class UserRepository : TaskManagerDbRepository<User, ApplicationContext>
    {
        public UserRepository(ApplicationContext context) : base(context)
        {

        }

        public async Task<User?> GetUserAsync (string login, string password, CancellationToken cancellationToken)
        {
            return await context.Users
                .AsNoTracking()
                .Where(x => x.Login.Equals(login) && x.Password.Equals(password))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<User?> GetByLoginAsync (string login, CancellationToken cancellationToken)
        {
            return await context.Users
                .AsNoTracking()
                .Where(x => x.Login == login)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<AllUsersDto>> GetAllUserListAsync (CancellationToken cancellationToken)
        {
            return await context.Users
                .AsNoTracking()
                .Select(x => new AllUsersDto
                {
                    Id = x.Id,
                    UserName = x.UserName
                }).ToListAsync(cancellationToken);
        }
    }
}
