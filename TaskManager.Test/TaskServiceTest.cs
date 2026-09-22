using FluentAssertions;
using Moq;
using FluentValidation;
using TaskManager.DB;
using TaskManager.DB.DataModels;
using TaskManager.DB.DTO;
using TaskManager.Enums;
using TaskManager.Services;
using TaskManager.Services.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace TaskManager.Test
{
    public class TaskServiceTest
    {
        private readonly Mock<ITaskRepository> _repositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IExportService> _exportServiceMock;
        private readonly Mock<IApplicationMetrics> _metricMock;
        private readonly TaskService _sut;

        public TaskServiceTest()
        {
            _repositoryMock = new Mock<ITaskRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _exportServiceMock = new Mock<IExportService>();
            _metricMock = new Mock<IApplicationMetrics>();
            _sut = new TaskService(_repositoryMock.Object, _userRepositoryMock.Object, _exportServiceMock.Object, _metricMock.Object);
        }

        [Fact]
        public async Task CreateTaskAsync_CreatedWithValidData()
        {
            // Arrange
            var data = new CreateTaskDto
            {
                Title = "Test",
                Description = "Test",
                Priority = PriorityEnum.Medium
            };

            var createdTask = new DB.DataModels.Task
            {
                Id = 1,
                Title = data.Title,
                Description = data.Description,
                Priority = data.Priority
            };

            _repositoryMock
                .Setup(x => x.AddAsync(
                    It.IsAny<DB.DataModels.Task>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdTask);

            // Act
            var result = await _sut.CreateTaskAsync(data, CancellationToken.None);

            // Assert
            Assert.Equal(1, result.Id);
            Assert.Equal("Test", result.Title);
            Assert.Equal("Test", result.Description);
            Assert.Equal(PriorityEnum.Medium, result.Priority);
        }

        [Fact]
        public async Task CreateTaskAsync_ThrownValidationError_InvalidData()
        {
            // Arrange
            var data = new CreateTaskDto
            {
                Title = "",
                Description = "Test description",
                Priority = PriorityEnum.Medium
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _sut.CreateTaskAsync(data, CancellationToken.None));
        }

        [Fact]
        public async Task UpdateTaskAsync_UpdateWithValidData()
        {
            // Arrange
            int id = 1;

            var data = new UpdateTaskDto
            {
                Title = "New Test",
                Description = "New Test description",
                Priority = PriorityEnum.Medium,
                DueDate = Convert.ToDateTime("2026-10-15T00:00:00"),
                AssignedTo = 3
            };

            var existingTask = new DB.DataModels.Task
            {
                Id = id,
                Title = "Test",
                Description = "Test description",
                Priority = PriorityEnum.Medium,
                Status = StatusEnum.Created,
                DueDate = Convert.ToDateTime("2026-10-15T00:00:00"),
                AssignedTo = 3
            };

            var user = new User
            {
                Id = 3,
                Login = "testuser@test.com",
                Password = "hashpassword",
                UserName = "Test",
                Position = "Worker"
            };

            _repositoryMock
                .Setup(x => x.GetAsync(
                    id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTask);

            _userRepositoryMock
                .Setup(x => x.GetAsync(
                    data.AssignedTo!.Value,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _repositoryMock.Setup(x => x.UpdateAsync(
                It.IsAny<DB.DataModels.Task>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTask);

            // Act
            var result = await _sut.UpdateTaskAsync(id, data, CancellationToken.None);

            // Assert
            Assert.True(result);

            _repositoryMock.Verify(
                x => x.UpdateAsync(
                    It.Is<DB.DataModels.Task>(task =>
                        task.Id == id &&
                        task.Title == data.Title &&
                        task.Description == data.Description &&
                        task.Priority == data.Priority &&
                        task.DueDate == data.DueDate &&
                        task.AssignedTo == data.AssignedTo),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateTaskAsync_ReturnFalse_TaskIdNotExisted()
        {
            // Arrange
            int id = 1;

            var data = new UpdateTaskDto
            {
                Title = "Test",
                Description = "Test description",
                Priority = PriorityEnum.Medium,
                DueDate = Convert.ToDateTime("2026-10-15T00:00:00"),
                AssignedTo = 3
            };

            _repositoryMock
                .Setup(x => x.GetAsync(
                    id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((DB.DataModels.Task?)null);

            // Act
            var result = await _sut.UpdateTaskAsync(id, data, CancellationToken.None);

            // Assert
            Assert.False(result);

            _repositoryMock.Verify(
                x => x.UpdateAsync(
                    It.IsAny<DB.DataModels.Task>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateTaskAsync_ReturnFalse_UserIdNotExisted()
        {
            // Arrange
            int id = 1;

            var data = new UpdateTaskDto
            {
                Title = "Test",
                Description = "Test description",
                Priority = PriorityEnum.Medium,
                DueDate = Convert.ToDateTime("2026-10-15T00:00:00"),
                AssignedTo = 3
            };

            var existingTask = new DB.DataModels.Task
            {
                Id = id,
                Title = "Existing task",
                Description = "Existing description",
                Priority = PriorityEnum.Low,
                Status = StatusEnum.Created,
                CreatedAt = DateTime.Now
            };

            _repositoryMock
                .Setup(x => x.GetAsync(
                    id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTask);

            _userRepositoryMock
                .Setup(x => x.GetAsync(
                    data.AssignedTo!.Value,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _sut.UpdateTaskAsync(id, data, CancellationToken.None);

            // Assert
            Assert.False(result);

            _repositoryMock.Verify(
                x => x.UpdateAsync(
                    It.IsAny<DB.DataModels.Task>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateStatusAsync_ReturnTrue_ValidData()
        {
            // Arrange
            int id = 1;
            var status = StatusEnum.Complete;

            _repositoryMock.Setup(x => x.ChangeTaskStatusAsync(id, status,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.UpdateStatusAsync(id, status, CancellationToken.None);

            // Assert
            Assert.True(result);

            _repositoryMock.Verify(
                x => x.ChangeTaskStatusAsync(
                    id,
                    status,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateStatusAsync_ReturnFalse_InvalidTaskId()
        {
            // Arrange
            int id = 1;
            var status = StatusEnum.Complete;

            _repositoryMock.Setup(x => x.ChangeTaskStatusAsync(id, status,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.UpdateStatusAsync(id, status, CancellationToken.None);

            // Assert
            Assert.False(result);

            _repositoryMock.Verify(
                x => x.ChangeTaskStatusAsync(
                    id,
                    status,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteTaskAsync_ReturnTrue_ValidData()
        {
            // Arrange
            int id = 1;

            _repositoryMock.Setup(x => x.DeleteTaskAsync(
                id, 
                It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteTaskAsync(id, CancellationToken.None);

            // Assert
            Assert.True(result);

            _repositoryMock.Verify(
                x => x.DeleteTaskAsync(
                    id,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteTaskAsync_ReturnFalse_InvalidTaskId()
        {
            // Arrange
            int id = 1;

            _repositoryMock.Setup(x => x.DeleteTaskAsync(
                id,
                It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.DeleteTaskAsync(id, CancellationToken.None);

            // Assert
            Assert.False(result);

            _repositoryMock.Verify(
                x => x.DeleteTaskAsync(
                    id,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAllTasksAsync_SuccessReturnData()
        {
            // Arrange
            int page = 1;
            int pageSize = 500;

            var data = new List<AllTasksDto>
            {
                new AllTasksDto
                {
                    Id = 1,
                    Title = "Test task 1",
                    AssignedUser = "Worker 1",
                    Status = "InProgress"
                },
                new AllTasksDto
                {
                    Id = 2,
                    Title = "Test task 2",
                    AssignedUser = "Worker 2",
                    Status = "InProgress"
                }
            };

            _repositoryMock.Setup(x => x.GetAllTasksAsync(
                page,
                pageSize,
                It.IsAny<CancellationToken>()
                )).ReturnsAsync(data);

            // Act
            var result = await _sut.GetAllTasksAsync(page, pageSize, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            result.Should().BeEquivalentTo(data);

            _repositoryMock.Verify(
                x => x.GetAllTasksAsync(
                    page,
                    pageSize,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAllTasksAsync_ThrowLimitException()
        {
            // Arrange
            int page = 1;
            int pageSize = 501;

            // Act & Assert
            await Assert.ThrowsAsync<RequestLimitExceeded>(
                () => _sut.GetAllTasksAsync(page, pageSize, CancellationToken.None));

            _repositoryMock.Verify(
                x => x.GetAllTasksAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetAllTasksByUserIdAsync_SuccessReturnData()
        {
            // Arrange
            int userId = 1;
            int page = 1;
            int pageSize = 500;

            var data = new List<AllTasksDto>
            {
                new AllTasksDto
                {
                    Id = 1,
                    Title = "Test task 1",
                    AssignedUser = "Worker 1",
                    Status = "InProgress"
                },
                new AllTasksDto
                {
                    Id = 2,
                    Title = "Test task 2",
                    AssignedUser = "Worker 2",
                    Status = "InProgress"
                }
            };

            _repositoryMock.Setup(x => x.GetAllTasksByUserAsync(
                userId,
                page,
                pageSize,
                It.IsAny<CancellationToken>()
                )).ReturnsAsync(data);

            // Act
            var result = await _sut.GetAllTasksByUserIdAsync(userId, page, pageSize, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            result.Should().BeEquivalentTo(data);

            _repositoryMock.Verify(
                x => x.GetAllTasksByUserAsync(
                    userId,
                    page,
                    pageSize,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ReturnTaskWithUser_WhenTaskAndUserExist()
        {
            // Arrange
            int id = 1;

            var user = new User
            {
                Id = 3,
                Login = "testuser@test.com",
                Password = "hashpassword",
                UserName = "Test",
                Position = "Worker"
            };

            var existingTask = new DB.DataModels.Task
            {
                Id = id,
                Title = "Test",
                Description = "Test description",
                Priority = PriorityEnum.Medium,
                Status = StatusEnum.Created,
                DueDate = Convert.ToDateTime("2026-10-15T00:00:00"),
                AssignedTo = 3
            };

            _repositoryMock
                .Setup(x => x.GetAsync(
                    id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTask);

            _userRepositoryMock
                .Setup(x => x.GetAsync(
                    existingTask.AssignedTo!.Value,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _sut.GetTaskByIdAsync(id, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(existingTask);
            result!.AssignedUser.Should().BeEquivalentTo(user);

            _repositoryMock.Verify(
                x => x.GetAsync(
                    id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _userRepositoryMock.Verify(
                 x => x.GetAsync(
                    user.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ReturnNull_WhenTaskNotExists()
        {
            // Arrange
            int id = 1;

            _repositoryMock
                .Setup(x => x.GetAsync(
                    id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((DB.DataModels.Task?)null);

            // Act
            var result = await _sut.GetTaskByIdAsync(id, CancellationToken.None);

            // Assert
            result.Should().BeNull();

            _repositoryMock.Verify(
                x => x.GetAsync(
                    id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _userRepositoryMock.Verify(
                 x => x.GetAsync(
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ReturnTask_WhenUserNotExists()
        {
            int id = 1;

            var existingTask = new DB.DataModels.Task
            {
                Id = id,
                Title = "Test",
                Description = "Test description",
                Priority = PriorityEnum.Medium,
                Status = StatusEnum.Created,
                DueDate = Convert.ToDateTime("2026-10-15T00:00:00"),
                AssignedTo = 3
            };

            _repositoryMock
                .Setup(x => x.GetAsync(
                    id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTask);

            _userRepositoryMock
                .Setup(x => x.GetAsync(
                    existingTask.AssignedTo!.Value,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _sut.GetTaskByIdAsync(id, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(existingTask);
            result!.AssignedUser.Should().BeNull();

            _repositoryMock.Verify(
                x => x.GetAsync(
                    id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _userRepositoryMock.Verify(
                 x => x.GetAsync(
                    existingTask.AssignedTo!.Value,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ReturnTask_WhenTaskIsNotAssigned()
        {
            // Arrange
            int id = 1;

            var existingTask = new DB.DataModels.Task
            {
                Id = id,
                Title = "Test",
                Description = "Test description",
                Priority = PriorityEnum.Medium,
                Status = StatusEnum.Created,
                DueDate = Convert.ToDateTime("2026-10-15T00:00:00"),
                AssignedTo = null
            };

            _repositoryMock
                .Setup(x => x.GetAsync(
                    id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTask);

            // Act
            var result = await _sut.GetTaskByIdAsync(id, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(existingTask);
            result!.AssignedUser.Should().BeNull();

            _userRepositoryMock.Verify(
                 x => x.GetAsync(
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ExportTasksAsync_ReturnFile_WhenDataIsValid()
        {
            // Arrange
            int page = 1;
            int pageSize = 500;

            var data = new List<TaskExportDto>
            {
                new TaskExportDto
                {
                    Title = "Test task 1",
                    Description = "Test task 1 description",
                    Status = "InProgress",
                    Priority = "Medium",
                    CreatedAt = "2026-09-16T00:00:00",
                    DueDate = "2026-10-16T00:00:00",
                    AssignedUser = "Worker 1"
                    
                },
                new TaskExportDto
                {
                    Title = "Test task 2",
                    Description = "Test task 2 description",
                    Status = "InProgress",
                    Priority = "Medium",
                    CreatedAt = "2026-09-16T00:00:00",
                    DueDate = "2026-10-16T00:00:00",
                    AssignedUser = "Worker 2"
                }
            };

            byte[] file = [1, 2, 3, 4];

            _repositoryMock.Setup(x => x.GetTasksForExport(page, pageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(data);

            _exportServiceMock.Setup(x => x.ExportToCsvAsync(data, It.IsAny<CancellationToken>())).ReturnsAsync(file);

            // Act
            var result = await _sut.ExportTasksAsync(page, pageSize, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(file);

            _repositoryMock.Verify(
                x => x.GetTasksForExport(
                    page,
                    pageSize,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _exportServiceMock.Verify(
                x => x.ExportToCsvAsync(
                    data,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ExportTasksAsync_ThrowsRequestLimitExceeded_WhenPageSizeIsGreaterThan500()
        {
            // Arrange
            int page = 1;
            int pageSize = 501;

            // Act & Assert
            await Assert.ThrowsAsync<RequestLimitExceeded>(
                () => _sut.ExportTasksAsync(page, pageSize, CancellationToken.None));

            _repositoryMock.Verify(
                x => x.GetTasksForExport(
                    page,
                    pageSize,
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _exportServiceMock.Verify(
                x => x.ExportToCsvAsync(
                    It.IsAny<List<TaskExportDto>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}