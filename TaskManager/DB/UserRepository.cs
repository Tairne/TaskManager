using Microsoft.EntityFrameworkCore;
using TaskManager.DB.DataModels;

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
    }
}
