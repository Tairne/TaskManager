using FluentAssertions;
using FluentValidation;
using Moq;
using TaskManager.DB;
using TaskManager.DB.DataModels;
using TaskManager.DB.DTO;
using TaskManager.Services;
using TaskManager.Services.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Task = System.Threading.Tasks.Task;

namespace TaskManager.Test
{
    public class AccountServiceTest
    {
        private readonly Mock<IUserRepository> _repositoryMock;
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly AccountService _sut;

        public AccountServiceTest()
        {
            _repositoryMock = new Mock<IUserRepository>();
            _passwordServiceMock = new Mock<IPasswordService>();
            _sut = new AccountService(_repositoryMock.Object, _passwordServiceMock.Object);
        }

        [Fact]
        public async Task CreateAccountAsync_UserNotAlreadyExist_ValidData()
        {
            // Arrange
            string hash = "hashpassword";
            
            var data = new CreateUserDto
            {
                Login = "test@test.com",
                Password = "password",
                UserName = "Test Test",
                Position = "Test"
            };

            var createdUser = new User
            {
                Id = 1,
                Login = data.Login,
                Password = hash,
                UserName = data.UserName,
                Position = data.Position,
                Role = "User",
                RefreshToken = null,
                RefreshTokenExpiryTime = null
            };

            _repositoryMock.Setup(x => x.GetByLoginAsync(data.Login, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            _passwordServiceMock.Setup(x => x.Hash(data.Password)).Returns(hash);

            _repositoryMock.Setup(x => x.AddAsync(
                It.Is<User>(user =>
                    user.Login == data.Login &&
                    user.Password == hash &&
                    user.UserName == data.UserName &&
                    user.Position == data.Position), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

            // Act
            var result = await _sut.CreateAccountAsync(data, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(createdUser);

            _repositoryMock.Verify(x => x.AddAsync(
                It.Is<User>(user =>
                    user.Login == data.Login &&
                    user.Password == hash &&
                    user.UserName == data.UserName &&
                    user.Position == data.Position &&
                    user.Role == "User"),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateAccountAsync_DuplicateLoginException_UserIsAlreadyExist()
        {
            // Arrange
            var data = new CreateUserDto
            {
                Login = "test@test.com",
                Password = "password",
                UserName = "Test Test",
                Position = "Test"
            };

            var existingUser = new User
            {
                Id = 1,
                Login = data.Login,
                Password = "hashpassword",
                UserName = data.UserName,
                Position = data.Position
            };

            _repositoryMock.Setup(x => x.GetByLoginAsync(data.Login, It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);

            // Act & Assert
            await Assert.ThrowsAsync<DuplicateLoginException>(() => _sut.CreateAccountAsync(data, CancellationToken.None));

            _repositoryMock.Verify(x => x.AddAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAccountAsync_ValidationError_InvalidData()
        {
            // Arrange
            var data = new CreateUserDto
            {
                Login = "testtest.com",
                Password = "pass",
                UserName = "T"
            };

            _repositoryMock.Setup(x => x.GetByLoginAsync(data.Login, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAccountAsync(data, CancellationToken.None));

            _repositoryMock.Verify(x => x.AddAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAccountAsync_ReturnTrue_UserExisted_ValidData()
        {
            // Arrange
            int id = 1;

            var data = new UpdateUserDto
            {
                Login = "test@test.com",
                UserName = "New Username",
                Position = "Test"
            };

            var existingUser = new User
            {
                Id = id,
                Login = data.Login,
                Password = "hashpassword",
                UserName = "Test Test",
                Position = data.Position
            };

            _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);

            // Act
            var result = await _sut.UpdateAccountAsync(id, data, CancellationToken.None);

            // Assert
            Assert.True(result);

            _repositoryMock.Verify(x => x.UpdateAsync(
                It.Is<User>(user =>
                    user.Login == data.Login &&
                    user.UserName == data.UserName &&
                    user.Position == data.Position),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAccountAsync_ReturnFalse_UserNotExist()
        {
            // Arrange
            int id = 1;

            var data = new UpdateUserDto
            {
                Login = "test@test.com",
                UserName = "Test Test",
                Position = "Test"
            };

            _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            // Act
            var result = await _sut.UpdateAccountAsync(id, data, CancellationToken.None);

            // Assert
            Assert.False(result);

            _repositoryMock.Verify(x => x.UpdateAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAccountAsync_ValidationError_InvalidData()
        {
            // Arrange
            int id = 1;

            var data = new UpdateUserDto
            {
                Login = "testtest.com",
                UserName = "f",
                Position = "Test"
            };

            var existingUser = new User
            {
                Id = id,
                Login = data.Login,
                Password = "hashpassword",
                UserName = "Test Test",
                Position = data.Position
            };

            _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateAccountAsync(id, data, CancellationToken.None));

            _repositoryMock.Verify(x => x.UpdateAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_ReturnTrue_UserExists_ValidData()
        {
            int id = 1;
            string hash = "newhashpassword";

            var data = new ChangePasswordDto
            {
                CurrentPassword = "password",
                NewPassword = "newpassword"
            };

            var user = new User
            {
                Id = id,
                Password = "hashpassword"
            };

            _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _passwordServiceMock.Setup(x => x.Verify(data.CurrentPassword, user.Password)).Returns(true);
            _passwordServiceMock.Setup(x => x.Hash(data.NewPassword)).Returns(hash);

            // Act
            var result = await _sut.ChangePasswordAsync(id, data, CancellationToken.None);

            // Assert
            Assert.True(result);
            _repositoryMock.Verify(x => x.UpdateAsync(
                It.Is<User>(user =>
                    user.Password == hash),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_ReturnFalse_InvalidCurrentPassword()
        {
            // Arrange
            int id = 1;
            string hash = "newhashpassword";

            var data = new ChangePasswordDto
            {
                CurrentPassword = "password",
                NewPassword = "newpassword"
            };

            var user = new User
            {
                Id = id,
                Password = "hashpassword"
            };

            _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _passwordServiceMock.Setup(x => x.Verify(data.CurrentPassword, user.Password)).Returns(false);

            // Act
            var result = await _sut.ChangePasswordAsync(id, data, CancellationToken.None);

            // Assert
            Assert.False(result);

            _repositoryMock.Verify(x => x.UpdateAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_ValidationException_InvalidData()
        {
            // Arrange
            int id = 1;

            var data = new ChangePasswordDto
            {
                CurrentPassword = "password",
                NewPassword = "newpwd"
            };

            var user = new User
            {
                Id = id,
                Password = "hashpassword"
            };

            _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _sut.ChangePasswordAsync(id, data, CancellationToken.None));

            _repositoryMock.Verify(x => x.UpdateAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_KeyNotFoundException_UserNotExists()
        {
            // Arrange
            int id = 1;

            var data = new ChangePasswordDto
            {
                CurrentPassword = "password",
                NewPassword = "newpwd"
            };

            _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.ChangePasswordAsync(id, data, CancellationToken.None));

            _repositoryMock.Verify(x => x.UpdateAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetAllAsync_AlwaysSuccessful()
        {
            // Arrange
            List<AllUsersDto> users = new List<AllUsersDto>
            {
                new AllUsersDto
                {
                    Id = 1,
                    UserName = "Test1",
                },
                new AllUsersDto
                {
                    Id = 2,
                    UserName = "Test2"
                }
            };

            _repositoryMock.Setup(x => x.GetAllUserListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(users);

            // Act
            var result = await _sut.GetAllAsync(CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(users);

            _repositoryMock.Verify(x => x.GetAllUserListAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_AlwaysSuccessful()
        {
            // Arrange
            int id = 1;

            var user = new User
            {
                Id = id,
                Login = "test@test.com"
            };

            _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            // Act
            var result = await _sut.GetByIdAsync(id, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(user);

            _repositoryMock.Verify(x => x.GetAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
