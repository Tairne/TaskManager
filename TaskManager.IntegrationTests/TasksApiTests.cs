using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using TaskManager.DB;
using TaskManager.DB.DataModels;
using TaskManager.DB.DTO;
using TaskManager.Enums;
using TaskManager.IntegrationTests.Infrastructure;
using Task = System.Threading.Tasks.Task;

namespace TaskManager.IntegrationTests
{
    public class TasksApiTests : IntegrationTestBase, IClassFixture<IntegrationFixture>
    {
        public TasksApiTests(IntegrationFixture fixture)
        : base(fixture)
        {
        }

        [Fact]
        public async Task CreateTasks_Manager_TasksAreSavedToDatabase()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync(
                "test-manager",
                "Test123!");

            var tasks = new[]
            {
                new CreateTaskDto
                {
                    Title = "First integration task",
                    Description = "First test task",
                    Priority = PriorityEnum.Medium
                },
                new CreateTaskDto
                {
                    Title = "Second integration task",
                    Description = "Second test task",
                    Priority = PriorityEnum.High
                },
                new CreateTaskDto
                {
                    Title = "Third integration task",
                    Description = "Third test task",
                    Priority = PriorityEnum.Low
                }
            };

            // Act
            foreach (var task in tasks)
            {
                var response = await client.PostAsJsonAsync(
                    "/api/tasks",
                    task);

                // Assert
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);

                var createdTask = await response.Content.ReadFromJsonAsync<DB.DataModels.Task>();

                Assert.NotNull(createdTask);
            }

            using var scope = Factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationContext>();

            var savedTasks = await db.Set<DB.DataModels.Task>()
                .Where(x =>
                    x.Title == "First integration task" ||
                    x.Title == "Second integration task" ||
                    x.Title == "Third integration task")
                .ToListAsync();

            Assert.Equal(3, savedTasks.Count);

            Assert.Contains(
                savedTasks,
                x => x.Title == "First integration task");

            Assert.Contains(
                savedTasks,
                x => x.Title == "Second integration task");

            Assert.Contains(
                savedTasks,
                x => x.Title == "Third integration task");
        }

        [Fact]
        public async Task CreateTasks_User_ReturnsForbidden()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync(
                "test-user",
                "Test123!");

            var task = new CreateTaskDto
            {
                Title = "Should not be created",
                Description = "This task must not be created",
                Priority = PriorityEnum.Medium
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/tasks", task);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            using var scope = Factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationContext>();

            var exists = await db.Set<DB.DataModels.Task>()
                .AnyAsync(x => x.Title == "Should not be created");

            Assert.False(exists);
        }

        [Fact]
        public async Task UpdateTask_Manager_UpdateSaveToDatabase()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync(
                "test-manager",
                "Test123!");

            var createRequest = new CreateTaskDto
            {
                Title = "Created title",
                Description = "Created description",
                Priority = PriorityEnum.Medium
            };

            var task = await CreateTaskAsync(client, createRequest);

            using var scope = Factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationContext>();

            var assignUser = await db.Users
                .SingleAsync(x => x.Login == "assigned-user");

            var updateRequest = new UpdateTaskDto
            {
                Title = "Updated title",
                Description = "Updated description",
                Priority = PriorityEnum.High,
                DueDate = DateTime.Now.AddDays(15),
                AssignedTo = assignUser.Id
            };

            // Act
            var updateResponse = await client.PutAsJsonAsync($"/api/tasks/{task.Id}", updateRequest);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

            var updatedTask = await db.Set<DB.DataModels.Task>().SingleAsync(x => x.Id == task.Id);

            Assert.Equal("Updated title", updatedTask.Title);
            Assert.Equal("Updated description", updatedTask.Description);
            Assert.Equal(PriorityEnum.High, updatedTask.Priority);
            Assert.Equal(assignUser.Id, updatedTask.AssignedTo);
        }

        [Fact]
        public async Task UpdateTask_User_ReturnsForbidden()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync(
                "test-user",
                "Test123!");

            var updateRequest = new UpdateTaskDto
            {
                Title = "Updated title",
                Description = "Updated description",
                Priority = PriorityEnum.High,
                DueDate = DateTime.Now.AddDays(15)
            };

            // Act
            var updateResponse = await client.PutAsJsonAsync($"/api/tasks/1", updateRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);
        }

        [Fact]
        public async Task UpdateTask_TaskNotExists()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync(
                "test-manager",
                "Test123!");

            using var scope = Factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationContext>();

            var assignUser = await db.Users
                .SingleAsync(x => x.Login == "assigned-user");

            var updateRequest = new UpdateTaskDto
            {
                Title = "Updated title",
                Description = "Updated description",
                Priority = PriorityEnum.High,
                DueDate = DateTime.Now.AddDays(15),
                AssignedTo = assignUser.Id
            };

            // Act
            var updateResponse = await client.PutAsJsonAsync($"/api/tasks/0", updateRequest);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
        }

        [Fact]
        public async Task UpdateTask_AssignedUserNotExist()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync(
                "test-manager",
                "Test123!");

            var createRequest = new CreateTaskDto
            {
                Title = "Created title",
                Description = "Created description",
                Priority = PriorityEnum.Medium
            };

            var task = await CreateTaskAsync(client, createRequest);

            var updateRequest = new UpdateTaskDto
            {
                Title = "Updated title",
                Description = "Updated description",
                Priority = PriorityEnum.High,
                DueDate = DateTime.Now.AddDays(15),
                AssignedTo = 0
            };

            // Act
            var updateResponse = await client.PutAsJsonAsync($"/api/tasks/{task.Id}", updateRequest);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);

            using var scope = Factory.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            var updatedTask = await db.Set<DB.DataModels.Task>().SingleAsync(x => x.Id == task.Id);

            Assert.NotEqual("Updated title", updatedTask.Title);
            Assert.NotEqual("Updated description", updatedTask.Description);
        }

        [Fact]
        public async Task UpdateStatus_StatusUpdated()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync(
                "test-manager",
                "Test123!");

            var createRequest = new CreateTaskDto
            {
                Title = "Created title",
                Description = "Created description",
                Priority = PriorityEnum.Medium
            };

            var task = await CreateTaskAsync(client, createRequest);

            // Act
            var response = await client.PatchAsync($"/api/tasks/{task.Id}?status=InProgress", null);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            using var scope = Factory.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            var updatedTask = await db.Set<DB.DataModels.Task>().SingleAsync(x => x.Id == task.Id);

            Assert.Equal(StatusEnum.InProgress, updatedTask.Status);
        }

        [Fact]
        public async Task UpdateStatus_NotFoundTask()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync(
                "test-manager",
                "Test123!");

            // Act
            var response = await client.PatchAsync($"/api/tasks/{int.MaxValue}?status=InProgress", null);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteTask_TaskDeleted()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync(
                "test-admin",
                "Test123!");

            var createRequest = new CreateTaskDto
            {
                Title = "Created title",
                Description = "Created description",
                Priority = PriorityEnum.Medium
            };

            var task = await CreateTaskAsync(client, createRequest);

            // Act
            var response = await client.DeleteAsync($"/api/tasks/{task.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            using var scope = Factory.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            var deletedTask = await db.Set<DB.DataModels.Task>().SingleOrDefaultAsync(x => x.Id == task.Id);

            Assert.Null(deletedTask);
        }

        [Fact]
        public async Task DeleteTask_Manager_AccessForbiden()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync(
                "test-manager",
                "Test123!");

            var createRequest = new CreateTaskDto
            {
                Title = "Created title",
                Description = "Created description",
                Priority = PriorityEnum.Medium
            };

            var task = await CreateTaskAsync(client, createRequest);

            // Act
            var response = await client.DeleteAsync($"/api/tasks/{task.Id}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            using var scope = Factory.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            var deletedTask = await db.Set<DB.DataModels.Task>().SingleOrDefaultAsync(x => x.Id == task.Id);

            Assert.NotNull(deletedTask);
        }

        [Fact]
        public async Task DeleteTask_TaskNotFound()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync(
                "test-admin",
                "Test123!");

            // Act
            var response = await client.DeleteAsync($"/api/tasks/{int.MaxValue}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAllTasksAsync_PageSize2_ReturnsTwoTasks()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync(
                "test-manager",
                "Test123!");

            for (int i = 1; i <= 5; i++)
            {
                await CreateTaskAsync(client, new CreateTaskDto
                {
                    Title = $"Pagination Task {i}",
                    Priority = PriorityEnum.Medium
                });
            }

            // Act
            var response = await client.GetAsync(
                "/api/tasks?page=1&size=2");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var tasks = await response.Content
                .ReadFromJsonAsync<List<AllTasksDto>>();

            Assert.NotNull(tasks);
            Assert.Equal(2, tasks.Count);
        }

        [Fact]
        public async Task GetAllTasksAsync_PageSizeOverLimit()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync(
                "test-manager",
                "Test123!");

            // Act
            var response = await client.GetAsync(
                "/api/tasks?page=1&size=501");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAllTasksByUserAsync_ReturnsAllUserTasks()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync(
                "test-manager",
                "Test123!");

            using var scope = Factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationContext>();

            var assignUser = await db.Users
                .SingleAsync(x => x.Login == "assigned-user");

            var otherUser = await db.Users
                .SingleAsync(x => x.Login == "test-user");

            var otherUserTask = await CreateTaskAsync(client, new CreateTaskDto
            {
                Title = $"Other user task",
                Priority = PriorityEnum.Medium
            });

            await client.PutAsJsonAsync($"/api/tasks/{otherUserTask.Id}", new UpdateTaskDto { Title = otherUserTask.Title, Priority = otherUserTask.Priority, AssignedTo = otherUser.Id });

            for (int i = 1; i <= 4; i++)
            {
                var task = await CreateTaskAsync(client, new CreateTaskDto
                {
                    Title = $"Pagination Task {i}",
                    Priority = PriorityEnum.Medium
                });

                await client.PutAsJsonAsync($"/api/tasks/{task.Id}", new UpdateTaskDto { Title = task.Title, Priority = task.Priority, AssignedTo = assignUser.Id });
            }

            // Act
            var response = await client.GetAsync(
                $"/api/tasks/user?user={assignUser.Id}&page=1&size=5");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var tasks = await response.Content
                .ReadFromJsonAsync<List<AllTasksDto>>();

            Assert.NotNull(tasks);
            Assert.DoesNotContain(tasks, x => x.Id == otherUserTask.Id);
        }

        [Fact]
        public async Task GetAllTasksByUserAsync_ReturnsTwoTasks()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync(
                "test-manager",
                "Test123!");

            using var scope = Factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationContext>();

            var assignUser = await db.Users
                .SingleAsync(x => x.Login == "assigned-user");

            var otherUser = await db.Users
                .SingleAsync(x => x.Login == "test-user");

            var otherUserTask = await CreateTaskAsync(client, new CreateTaskDto
            {
                Title = $"Other user task",
                Priority = PriorityEnum.Medium
            });

            await client.PutAsJsonAsync($"/api/tasks/{otherUserTask.Id}", new UpdateTaskDto { Title = otherUserTask.Title, Priority = otherUserTask.Priority, AssignedTo = otherUser.Id });

            for (int i = 1; i <= 4; i++)
            {
                var task = await CreateTaskAsync(client, new CreateTaskDto
                {
                    Title = $"Pagination Task {i}",
                    Priority = PriorityEnum.Medium
                });

                await client.PutAsJsonAsync($"/api/tasks/{task.Id}", new UpdateTaskDto { Title = task.Title, Priority = task.Priority, AssignedTo = assignUser.Id });
            }

            // Act
            var response = await client.GetAsync(
                $"/api/tasks/user?user={assignUser.Id}&page=1&size=2");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var tasks = await response.Content
                .ReadFromJsonAsync<List<AllTasksDto>>();

            Assert.NotNull(tasks);
            Assert.DoesNotContain(tasks, x => x.Id == otherUserTask.Id);
            Assert.Equal(2, tasks.Count);
        }

        [Fact]
        public async Task GetTaskAsync_ExistedTaskReturns()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync(
                "test-manager",
                "Test123!");

            var createRequest = new CreateTaskDto
            {
                Title = "Created title",
                Description = "Created description",
                Priority = PriorityEnum.Medium
            };

            var task = await CreateTaskAsync(client, createRequest);

            // Act
            var response = await client.GetAsync(
                $"/api/tasks/{task.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var actualTask = await response.Content
                .ReadFromJsonAsync<DB.DataModels.Task>();

            Assert.NotNull(actualTask);
            Assert.Equal(task.Id, actualTask.Id);
            Assert.Equal(task.Title, actualTask.Title);
        }

        [Fact]
        public async Task GetTaskAsync_TaskNotExists()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync(
                "test-manager",
                "Test123!");

            // Act
            var response = await client.GetAsync(
                $"/api/tasks/{int.MaxValue}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Export_ReturnsCsvFile()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync(
                "test-manager",
                "Test123!");

            var task = await CreateTaskAsync(client, new CreateTaskDto
            {
                Title = "Export integration task",
                Priority = PriorityEnum.Medium
            });

            // Act
            var response = await client.GetAsync(
                "/api/tasks/csv?page=1&size=100");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.Equal(
                "text/csv",
                response.Content.Headers.ContentType?.MediaType);

            var csv = await response.Content.ReadAsStringAsync();

            Assert.False(string.IsNullOrWhiteSpace(csv));
            Assert.Contains(task.Title, csv);

            var disposition = response.Content.Headers.ContentDisposition;

            Assert.NotNull(disposition);
            Assert.Equal("attachment", disposition.DispositionType);
            Assert.StartsWith("Export-", disposition.FileNameStar ?? disposition.FileName);
            Assert.EndsWith(".csv", disposition.FileNameStar ?? disposition.FileName);
        }

        [Fact]
        public async Task Export_OverLimitRequest()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync(
                "test-manager",
                "Test123!");

            // Act
            var response = await client.GetAsync(
                "/api/tasks/csv?page=1&size=501");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
