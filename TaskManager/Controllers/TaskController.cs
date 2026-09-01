using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet]
        public async Task<ActionResult<List<AllTasksDto>>> GetAllTasksAsync([FromQuery] int page, [FromQuery(Name = "size")] int pageSize, CancellationToken cancellationToken)
        {
            var result = await _taskService.GetAllTasksAsync(page, pageSize, cancellationToken);
            return Ok(result);
        }

        [HttpGet("user")]
        public async Task<ActionResult<List<AllTasksDto>>> GetAllTasksByUserAsync([FromQuery(Name = "user")] int userId, [FromQuery] int page, [FromQuery(Name = "size")] int pageSize, CancellationToken cancellationToken)
        {
            var result = await _taskService.GetAllTasksByUserIdAsync(userId, page, pageSize, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DB.DataModels.Task>> GetTask([FromRoute] int id, CancellationToken cancellationToken)
        {
            var result = await _taskService.GetTaskByIdAsync(id, cancellationToken);

            if (result == null)
            {
                return NotFound($"Task id {id} not exists");
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<DB.DataModels.Task>> CreateTask([FromBody] CreateTaskDto task, CancellationToken cancellationToken)
        {
            var result = await _taskService.CreateTaskAsync(task, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask([FromRoute] int id, [FromBody] UpdateTaskDto task, CancellationToken cancellationToken)
        {
            bool result = await _taskService.UpdateTaskAsync(id, task, cancellationToken);

            if (!result)
            {
                return NotFound($"Task id {id} not exists");
            }

            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateStatus([FromRoute] int id, [FromQuery] StatusEnum status, CancellationToken cancellationToken)
        {
            bool result = await _taskService.UpdateStatusAsync(id, status, cancellationToken);

            if (!result)
            {
                return NotFound($"Task id {id} not exists");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask([FromRoute] int id, CancellationToken cancellationToken)
        {
            bool result = await _taskService.DeleteTaskAsync(id, cancellationToken);

            if (!result)
            {
                return NotFound($"Task id {id} not exists");
            }

            return NoContent();
        }

        [HttpGet("csv")]
        public async Task<FileResult> Export([FromQuery] int page, [FromQuery(Name ="size")] int pageSize, CancellationToken cancellationToken)
        {
            var result = await _taskService.ExportTasksAsync(page, pageSize, cancellationToken);

            return File(result, "text/csv", $"Export-{DateTime.Now.ToString("s")}.csv");
        }
    }
}
