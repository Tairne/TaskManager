using TaskManager.DB.DTO;
using TaskManager.Enums;

namespace TaskManager.Services.Interfaces
{
    public interface ITaskService
    {
        Task<List<AllTasksDto>> GetAllTasksAsync(int page, int pageSize, CancellationToken cancellationToken);
        Task<List<AllTasksDto>> GetAllTasksByUserIdAsync(int userId, int page, int pageSize, CancellationToken cancellationToken);
        Task<DB.DataModels.Task?> GetTaskByIdAsync(int id, CancellationToken cancellationToken);
        Task<DB.DataModels.Task> CreateTaskAsync(CreateTaskDto taskData, CancellationToken cancellationToken);
        Task<bool> UpdateTaskAsync(int id, UpdateTaskDto taskData, CancellationToken cancellationToken);
        Task<bool> UpdateStatusAsync(int id, StatusEnum status, CancellationToken cancellationToken);
        Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken);
    }
}
