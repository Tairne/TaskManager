using TaskManager.DB;
using TaskManager.DB.DTO;
using TaskManager.Enums;
using TaskManager.Services.Interfaces;

namespace TaskManager.Services
{
    public class TaskService : ITaskService
    {
        private readonly TaskRepository _repository;
        private readonly UserRepository _userRepository;
        private readonly IExportService _exportService;
        private readonly IApplicationMetrics _metrics;

        public TaskService(TaskRepository repository, UserRepository userRepository, IExportService exportService, IApplicationMetrics metrics)
        {
            this._repository = repository;
            this._userRepository = userRepository;
            this._exportService = exportService;
            this._metrics = metrics;
        }

        public async Task<DB.DataModels.Task> CreateTaskAsync(CreateTaskDto taskData, CancellationToken cancellationToken)
        {
            var task = new DB.DataModels.Task
            {
                Title = taskData.Title,
                Description = taskData.Description,
                Priority = taskData.Priority
            };

            _metrics.TaskCreated();
            return await _repository.AddAsync(task, cancellationToken);
        }

        public async Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken)
        {
            return await _repository.DeleteTaskAsync(id, cancellationToken);
        }

        public async Task<List<AllTasksDto>> GetAllTasksAsync(int page, int pageSize, CancellationToken cancellationToken)
        {
            if (pageSize >= 500)
            {
                throw new RequestLimitExceeded(500);
            }

            return await _repository.GetAllTasksAsync(page, pageSize, cancellationToken);
        }

        public async Task<List<AllTasksDto>> GetAllTasksByUserIdAsync(int userId, int page, int pageSize, CancellationToken cancellationToken)
        {
            return await _repository.GetAllTasksByUserAsync(userId, page, pageSize, cancellationToken);
        }

        public async Task<DB.DataModels.Task?> GetTaskByIdAsync(int id, CancellationToken cancellationToken)
        {
            var task = await _repository.GetAsync(id, cancellationToken);
            var user = await _userRepository.GetAsync(id, cancellationToken);
            
            if (task != null && user != null)
            {
                task.AssignedUser = user;
            }

            return task;
        }

        public async Task<bool> UpdateStatusAsync(int id, StatusEnum status, CancellationToken cancellationToken)
        {
            if (status == StatusEnum.Complete)
            {
                _metrics.TaskCompleted();
            }

            return await _repository.ChangeTaskStatusAsync(id, status, cancellationToken);
        }

        public async Task<bool> UpdateTaskAsync(int id, UpdateTaskDto taskData, CancellationToken cancellationToken)
        {
            var task = await _repository.GetAsync(id, cancellationToken);

            if (task == null)
                return false;

            task.Title = taskData.Title;
            task.Description = taskData.Description;
            task.Priority = taskData.Priority;
            task.DueDate = taskData.DueDate;
            task.AssignedTo = taskData.AssignedTo;

            await _repository.UpdateAsync(task, cancellationToken);
            return true;
        }

        public async Task<byte[]> ExportTasksAsync(int page, int pageSize, CancellationToken cancellationToken)
        {
            if (pageSize >= 500)
            {
                throw new RequestLimitExceeded(500);
            }

            var tasks = await _repository.GetTasksForExport(page, pageSize, cancellationToken);

            return await _exportService.ExportToCsvAsync(tasks, cancellationToken);
        }
    }
}
