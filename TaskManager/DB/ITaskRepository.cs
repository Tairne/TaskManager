using TaskManager.DB.DTO;
using TaskManager.Enums;

namespace TaskManager.DB
{
    public interface ITaskRepository : IRepository<DataModels.Task>
    {
        Task<List<AllTasksDto>> GetAllTasksAsync(int page, int pageSize, CancellationToken cancellationToken);

        Task<List<AllTasksDto>> GetAllTasksByUserAsync(int userId, int page, int pageSize, CancellationToken cancellationToken);

        Task<bool> ChangeTaskStatusAsync(int id, StatusEnum status, CancellationToken cancellationToken);

        Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken);

        Task<List<TaskExportDto>> GetTasksForExport(int page, int pageSize, CancellationToken cancellationToken);
    }
}
