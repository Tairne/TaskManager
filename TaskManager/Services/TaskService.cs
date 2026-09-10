using FluentValidation;
using Serilog;
using TaskManager.DB;
using TaskManager.DB.DataModels;
using TaskManager.DB.DTO;
using TaskManager.Enums;
using TaskManager.Services.Interfaces;
using TaskManager.Validators;

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

            var validator = new CreateTaskValidator();
            var result = await validator.ValidateAsync(taskData, cancellationToken);

            if (!result.IsValid)
                throw new ValidationException(result.Errors);

            _metrics.TaskCreated();

            var create = await _repository.AddAsync(task, cancellationToken);

            Log.Information("Task Id: {0}, Title: {1} was created", create.Id, create.Title);

            return create;
        }

        public async Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken)
        {
            bool result = await _repository.DeleteTaskAsync(id, cancellationToken);

            if (result)
                Log.Information("Task Id {0} was deleted", id);

            return result;
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
            var result = await _repository.ChangeTaskStatusAsync(id, status, cancellationToken);

            if (result)
            {
                if (status == StatusEnum.Complete)
                {
                    _metrics.TaskCompleted();
                }

                Log.Information("Task Id {0} change status to {1}", id, status.ToString());
            }

            return result;
        }

        public async Task<bool> UpdateTaskAsync(int id, UpdateTaskDto taskData, CancellationToken cancellationToken)
        {
            var task = await _repository.GetAsync(id, cancellationToken);
            User? user = null;

            if (taskData.AssignedTo != null)
            {
                user = await _userRepository.GetAsync(taskData.AssignedTo.Value, cancellationToken);

                if (user == null)
                    return false;
            }

            if (task == null)
                return false;

            var validator = new UpdateTaskValidator(task.CreatedAt);
            var result = await validator.ValidateAsync(taskData, cancellationToken);

            if (!result.IsValid)
                throw new ValidationException(result.Errors);

            task.Title = taskData.Title;
            task.Description = taskData.Description;
            task.Priority = taskData.Priority;
            task.DueDate = taskData.DueDate;
            task.AssignedTo = taskData.AssignedTo;

            await _repository.UpdateAsync(task, cancellationToken);

            if (task.AssignedTo != null)
            {
                Log.Information("Task Id {0} assigned to user {1}", task.Id, user.Login);
            }

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
