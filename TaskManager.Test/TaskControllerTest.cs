using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Controllers;
using TaskManager.DB.DataModels;
using TaskManager.DB.DTO;
using TaskManager.Enums;
using TaskManager.Services.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Task = System.Threading.Tasks.Task;

namespace TaskManager.Test
{
    public class TaskControllerTest
    {
        private readonly Mock<ITaskService> _serviceMock;
        private readonly TaskController _sut;

        public TaskControllerTest()
        {
            _serviceMock = new Mock<ITaskService>();
            _sut = new TaskController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetAllTasksAsync_ReturnResult()
        {
            int page = 1;
            int pageSize = 10;

            List<AllTasksDto> tasks = new List<AllTasksDto>
            {
                new AllTasksDto
                {
                    Id = 1,
                    Title = "Test Title",
                    Status = "Created"
                },
                new AllTasksDto
                {
                    Id = 2,
                    Title = "Test Title 2",
                    Status = "Completed"
                }
            };

            _serviceMock.Setup(x => x.GetAllTasksAsync(page, pageSize, It.IsAny<CancellationToken>())).ReturnsAsync(tasks);

            var result = await _sut.GetAllTasksAsync(page, pageSize, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            okResult.Value.Should().BeEquivalentTo(tasks);

            _serviceMock.Verify(
                x => x.GetAllTasksAsync(
                    page,
                    pageSize,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAllTasksByUserAsync_ReturnResult()
        {
            int userId = 1;
            int page = 1;
            int pageSize = 10;

            List<AllTasksDto> tasks = new List<AllTasksDto>
            {
                new AllTasksDto
                {
                    Id = 1,
                    Title = "Test Title",
                    Status = "Created"
                },
                new AllTasksDto
                {
                    Id = 2,
                    Title = "Test Title 2",
                    Status = "Completed"
                }
            };

            _serviceMock.Setup(x => x.GetAllTasksByUserIdAsync(userId, page, pageSize, It.IsAny<CancellationToken>())).ReturnsAsync(tasks);

            var result = await _sut.GetAllTasksByUserAsync(userId, page, pageSize, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            okResult.Value.Should().BeEquivalentTo(tasks);

            _serviceMock.Verify(
                x => x.GetAllTasksByUserIdAsync(
                    userId,
                    page,
                    pageSize,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetTask_ReturnResult()
        {
            int id = 1;

            var task = new DB.DataModels.Task
            {
                Id = id,
                Title = "Test Title"
            };

            _serviceMock.Setup(x => x.GetTaskByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(task);

            var result = await _sut.GetTaskAsync(id, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            okResult.Value.Should().BeEquivalentTo(task);

            _serviceMock.Verify(
                x => x.GetTaskByIdAsync(
                    id,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAllTasksByUserAsync_TaskNotExists()
        {
            int id = 1;

            _serviceMock.Setup(x => x.GetTaskByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((DB.DataModels.Task?)null);

            var result = await _sut.GetTaskAsync(id, CancellationToken.None);

            Assert.IsType<NotFoundObjectResult>(result.Result);

            _serviceMock.Verify(
                x => x.GetTaskByIdAsync(
                    id,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateTask_TaskCreated()
        {
            var task = new CreateTaskDto
            {
                Title = "Title",
                Description = "Description",
                Priority = Enums.PriorityEnum.Medium
            };

            var createdTask = new DB.DataModels.Task
            {
                Id = 1,
                Title = "Title",
                Description = "Description",
                Priority = Enums.PriorityEnum.Medium
            };

            _serviceMock.Setup(x => x.CreateTaskAsync(task, It.IsAny<CancellationToken>())).ReturnsAsync(createdTask);

            var result = await _sut.CreateTask(task, CancellationToken.None);

            var response = Assert.IsType<ObjectResult>(result.Result);

            response.StatusCode.Should().Be(StatusCodes.Status201Created);
            response.Value.Should().BeEquivalentTo(createdTask);

            _serviceMock.Verify(
                x => x.CreateTaskAsync(
                    task,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateTask_SuccessUpdate()
        {
            int id = 1;

            var task = new UpdateTaskDto
            {
                Title = "Title",
                Description = "Description",
                Priority = Enums.PriorityEnum.Medium,
                AssignedTo = 1,
                DueDate = DateTime.Now.AddMonths(1)
            };

            _serviceMock.Setup(x => x.UpdateTaskAsync(id, task, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _sut.UpdateTask(id, task, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);

            _serviceMock.Verify(
                x => x.UpdateTaskAsync(
                    id,
                    task,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateTask_NotFoundUserOrTask()
        {
            int id = 1;

            var task = new UpdateTaskDto
            {
                Title = "Title",
                Description = "Description",
                Priority = Enums.PriorityEnum.Medium,
                AssignedTo = 1,
                DueDate = DateTime.Now.AddMonths(1)
            };

            _serviceMock.Setup(x => x.UpdateTaskAsync(id, task, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _sut.UpdateTask(id, task, CancellationToken.None);

            Assert.IsType<NotFoundObjectResult>(result);

            _serviceMock.Verify(
                x => x.UpdateTaskAsync(
                    id,
                    task,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_SuccessUpdate()
        {
            int id = 1;
            StatusEnum status = StatusEnum.Approval;

            _serviceMock.Setup(x => x.UpdateStatusAsync(id, status, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _sut.UpdateStatus(id, status, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);

            _serviceMock.Verify(
                x => x.UpdateStatusAsync(
                    id,
                    status,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_TaskNotFound()
        {
            int id = 1;
            StatusEnum status = StatusEnum.Approval;

            _serviceMock.Setup(x => x.UpdateStatusAsync(id, status, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _sut.UpdateStatus(id, status, CancellationToken.None);

            Assert.IsType<NotFoundObjectResult>(result);

            _serviceMock.Verify(
                x => x.UpdateStatusAsync(
                    id,
                    status,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteTask_Success()
        {
            int id = 1;

            _serviceMock.Setup(x => x.DeleteTaskAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _sut.DeleteTask(id, CancellationToken.None);
            
            Assert.IsType<NoContentResult>(result);

            _serviceMock.Verify(
                x => x.DeleteTaskAsync(
                    id,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteTask_TaskNotFound()
        {
            int id = 1;

            _serviceMock.Setup(x => x.DeleteTaskAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _sut.DeleteTask(id, CancellationToken.None);

            Assert.IsType<NotFoundObjectResult>(result);

            _serviceMock.Verify(
                x => x.DeleteTaskAsync(
                    id,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Export_Success()
        {
            int page = 1;
            int pageSize = 10;
            byte[] file = Encoding.UTF8.GetBytes("Title,Status\nTest,Created");

            _serviceMock.Setup(x => x.ExportTasksAsync(page, pageSize, It.IsAny<CancellationToken>())).ReturnsAsync(file);

            var result = await _sut.Export(page, pageSize, CancellationToken.None);

            var fileResult = Assert.IsType<FileContentResult>(result);

            fileResult.ContentType.Should().Be("text/csv");
            fileResult.FileContents.Should().BeEquivalentTo(file);
            fileResult.FileDownloadName.Should().StartWith("Export-");
            fileResult.FileDownloadName.Should().EndWith(".csv");

            _serviceMock.Verify(
                x => x.ExportTasksAsync(
                    page,
                    pageSize,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
