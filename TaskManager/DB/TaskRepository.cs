using Microsoft.EntityFrameworkCore;
using System.Threading;
using TaskManager.DB.DTO;
using TaskManager.Enums;

namespace TaskManager.DB
{
    public class TaskRepository : TaskManagerDbRepository<DataModels.Task,ApplicationContext>
    {
        public TaskRepository(ApplicationContext context) : base(context)
        {

        }

        public async Task<List<AllTasksDto>> GetAllTasksByUserAsync (int userId, int page, int pageSize, CancellationToken cancellationToken)
        {
            var tasks = await context.Tasks
                .Include(x => x.AssignedUser)
                .AsNoTracking()
                .Where(x => x.AssignedTo == userId)
                .Select(x => new AllTasksDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Status = x.Status.ToString(),
                    AssignedUser = x.AssignedUser != null ? x.AssignedUser.UserName : null,
                }).Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return tasks;
        }

        public async Task<bool> ChangeTaskStatusAsync(int id, StatusEnum status, CancellationToken cancellationToken)
        {
            var task = await context.Tasks.Where(x => x.Id == id).FirstAsync(cancellationToken);

            if (task == null)
                return false;

            task.Status = status;
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken)
        {
            var task = await context.Tasks.FindAsync(id, cancellationToken);

            if (task == null)
                return false;

            context.Tasks.Remove(task);
            await context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
