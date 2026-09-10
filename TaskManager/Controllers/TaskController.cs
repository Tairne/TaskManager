using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TaskManager.DB.DTO;
using TaskManager.Enums;
using TaskManager.Services.Interfaces;

namespace TaskManager.Controllers
{
    [Route("api/[controller]s")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            this._taskService = taskService;
        }

        /// <summary>
        /// Get all tasks with pagination
        /// </summary>
        /// <param name="page">Pagination page number</param>
        /// <param name="pageSize">Size of pagination page</param>
        /// <returns><see cref="List{AllTasksDto}"/></returns>
        /// <response code="200">All tasks list List{<a href='#model-AllTasksDto'>AllTasksDto</a>}</response>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<AllTasksDto>>> GetAllTasksAsync([FromQuery] int page, [FromQuery(Name = "size")] int pageSize, CancellationToken cancellationToken)
        {
            var result = await _taskService.GetAllTasksAsync(page, pageSize, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get all tasks for user
        /// </summary>
        /// <param name="userId">The Id of the user</param>
        /// <param name="page">Pagination page number</param>
        /// <param name="pageSize">Size of pagination page</param>
        /// <returns><see cref="List{AllTasksDto}"/></returns>
        /// <response code="200">All tasks list List{<a href='#model-AllTasksDto'>AllTasksDto</a>}</response>
        [Authorize]
        [HttpGet("user")]
        public async Task<ActionResult<List<AllTasksDto>>> GetAllTasksByUserAsync([FromQuery(Name = "user")] int userId, [FromQuery] int page, [FromQuery(Name = "size")] int pageSize, CancellationToken cancellationToken)
        {
            var result = await _taskService.GetAllTasksByUserIdAsync(userId, page, pageSize, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get task by Id
        /// </summary>
        /// <param name="id">The Id of the task</param>
        /// <returns><see cref="DB.DataModels.Task"/></returns>
        /// <response code="200">Task <a href='#model-DB.DataModels.Task'>Task</a></response>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<DB.DataModels.Task>> GetTask([FromRoute] int id, CancellationToken cancellationToken)
        {
            var result = await _taskService.GetTaskByIdAsync(id, cancellationToken);

            if (result == null)
            {
                Log.Error("Task id {id} not exists", id);
                return NotFound($"Task id {id} not exists");
            }

            return Ok(result);
        }

        /// <summary>
        /// Create task (only admins and managers allowed)
        /// </summary>
        /// <param name="task" example='{
        /// "title": "Test task title",
        /// "description": "Test task description",
        /// "priority": 0
        /// }'>Create task object: <a href='#model-CreateTaskDto'>CreateTaskDto</a></param>
        /// <remarks>Priority is PriorityEnum object:
        /// Medium = 0,Low = 1,High = 2</remarks>
        /// <returns><see cref="DB.DataModels.Task"/></returns>
        /// <response code="200">New task <a href='#model-DB.DataModels.Task'>Task</a></response>
        [Authorize(Policy = "CanManageTasks")]
        [HttpPost]
        public async Task<ActionResult<DB.DataModels.Task>> CreateTask([FromBody] CreateTaskDto task, CancellationToken cancellationToken)
        {
            var result = await _taskService.CreateTaskAsync(task, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>
        /// Update task: change title, description, priority, due date and assigned user (only admins and managers allowed)
        /// </summary>
        /// <param name="id">The Id of the task</param>
        /// <param name="task">Update task object:<a href='#model-UpdateTaskDto'>UpdateTaskDto</a></param>
        [Authorize(Policy = "CanManageTasks")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask([FromRoute] int id, [FromBody] UpdateTaskDto task, CancellationToken cancellationToken)
        {
            bool result = await _taskService.UpdateTaskAsync(id, task, cancellationToken);

            if (!result)
            {
                Log.Error("Task id {id} or assigned user not exists", id);
                return NotFound($"Task id {id} or assigned user not exists");
            }

            return NoContent();
        }

        /// <summary>
        /// Update task status
        /// </summary>
        /// <param name="id">The id of the task</param>
        /// <param name="status">Status that must be set</param>
        /// <remarks>Status is StatusEnum object:
        /// Created = 0,InProgress = 1,Approval = 2,Cancel = 3,Complete = 4
        /// Sample requests:
        ///
        ///     Patch /tasks
        ///     {
        ///         id: 66,
        ///         status: 4
        ///     }
        /// </remarks>
        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateStatus([FromRoute] int id, [FromQuery] StatusEnum status, CancellationToken cancellationToken)
        {
            bool result = await _taskService.UpdateStatusAsync(id, status, cancellationToken);

            if (!result)
            {
                Log.Error("Task id {id} not exists", id);
                return NotFound($"Task id {id} not exists");
            }

            return NoContent();
        }

        /// <summary>
        /// Delete task (only admins allowed)
        /// </summary>
        /// <param name="id">The id of the task that must be deleted</param>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask([FromRoute] int id, CancellationToken cancellationToken)
        {
            bool result = await _taskService.DeleteTaskAsync(id, cancellationToken);

            if (!result)
            {
                Log.Error("Task id {id} not exists", id);
                return NotFound($"Task id {id} not exists");
            }

            return NoContent();
        }

        /// <summary>
        /// Export tasks fields to csv file
        /// </summary>
        /// <param name="page">Pagination page number</param>
        /// <param name="pageSize">Size of pagination page</param>
        /// <remarks>Maximum 500 rows</remarks>
        [Authorize]
        [HttpGet("csv")]
        public async Task<FileResult> Export([FromQuery] int page, [FromQuery(Name ="size")] int pageSize, CancellationToken cancellationToken)
        {
            var result = await _taskService.ExportTasksAsync(page, pageSize, cancellationToken);

            return File(result, "text/csv", $"Export-{DateTime.Now.ToString("s")}.csv");
        }
    }
}
