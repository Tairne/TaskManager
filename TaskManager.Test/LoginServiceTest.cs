using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TaskManager.CommonModels;
using TaskManager.DB;
using TaskManager.DB.DataModels;
using TaskManager.DB.DTO;
using TaskManager.Services;
using TaskManager.Services.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace TaskManager.Test
{
    public class LoginServiceTest
    {
        private readonly Mock<IUserRepository> _repositoryMock;
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly LoginService _sut;

        public LoginServiceTest()
        {
            var jwtOptions = Options.Create(new JwtOptions
            {
                Key = "super-secret-key-for-unit-tests-123456789-super-secret-key-for-unit-tests-123456789-super-secret-key-for-unit-tests-123456789",
                Issuer = "TaskManager.Tests",
                Audience = "TaskManager.Tests",
                ExpiryInMinutes = 60
            });

            _repositoryMock = new Mock<IUserRepository>();
            _passwordServiceMock = new Mock<IPasswordService>();
            _sut = new LoginService(_repositoryMock.Object, _passwordServiceMock.Object, jwtOptions);
        }

        [Fact]
        public async Task LoginAsync_SuccessLogin()
        {
            string login = "test@test.com";
            string password = "password";

            var user = new User
            {
                Id = 1,
                Login = login,
                UserName = "Test Test",
                Role = "User",
                Position = "Test",
                Password = "hashpassword"
            };

            _repositoryMock.Setup(x => x.GetByLoginAsync(login, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _passwordServiceMock.Setup(x => x.Verify(password, user.Password)).Returns(true);
            _repositoryMock.Setup(x => x.SaveRefreshToken(user, It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

            var result = await _sut.LoginAsync(login, password, CancellationToken.None);

            result.Should().NotBeNull();
            result!.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(result.AccessToken);

            token.Claims.Should().ContainSingle(
                x => x.Type == ClaimTypes.NameIdentifier &&
                     x.Value == user.Id.ToString());

            token.Claims.Should().ContainSingle(
                x => x.Type == ClaimTypes.Name &&
                     x.Value == user.UserName);

            token.Claims.Should().ContainSingle(
                x => x.Type == ClaimTypes.Role &&
                     x.Value == user.Role);

            token.Claims.Should().ContainSingle(
                x => x.Type == "Position" &&
                     x.Value == user.Position);

            _repositoryMock.Verify(
                x => x.GetByLoginAsync(
                    login,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _passwordServiceMock.Verify(
                x => x.Verify(
                    password,
                    user.Password),
                Times.Once);

            _repositoryMock.Verify(
                x => x.SaveRefreshToken(
                    user,
                    It.Is<string>(token => !string.IsNullOrEmpty(token)),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword()
        {
            string login = "test@test.com";
            string password = "password";

            var user = new User
            {
                Id = 1,
                Login = login,
                UserName = "Test Test",
                Role = "User",
                Position = "Test",
                Password = "hashpassword"
            };

            _repositoryMock.Setup(x => x.GetByLoginAsync(login, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _passwordServiceMock.Setup(x => x.Verify(password, user.Password)).Returns(false);

            var result = await _sut.LoginAsync(login, password, CancellationToken.None);

            result.Should().BeNull();

            _repositoryMock.Verify(
                x => x.GetByLoginAsync(
                    login,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _passwordServiceMock.Verify(
                x => x.Verify(
                    password,
                    user.Password),
                Times.Once);

            _repositoryMock.Verify(
                x => x.SaveRefreshToken(
                    user,
                    It.Is<string>(token => !string.IsNullOrEmpty(token)),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_UserNotExists()
        {
            string login = "test@test.com";
            string password = "password";

            _repositoryMock.Setup(x => x.GetByLoginAsync(login, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            var result = await _sut.LoginAsync(login, password, CancellationToken.None);

            result.Should().BeNull();

            _repositoryMock.Verify(
                x => x.GetByLoginAsync(
                    login,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _passwordServiceMock.Verify(
                x => x.Verify(
                    password,
                    It.IsAny<string>()),
                Times.Never);

            _repositoryMock.Verify(
                x => x.SaveRefreshToken(
                    It.IsAny<User>(),
                    It.Is<string>(token => !string.IsNullOrEmpty(token)),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task RefreshTokenAsync_ValidRefreshToken()
        {
            var request = new RefreshTokenDto
            {
                UserId = 1,
                RefreshToken = "refreshtoken"
            };

            var user = new User
            {
                Id = request.UserId,
                RefreshToken = request.RefreshToken,
                RefreshTokenExpiryTime = DateTime.Now.AddDays(1)
            };

            _repositoryMock.Setup(x => x.GetAsync(request.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _repositoryMock.Setup(x => x.SaveRefreshToken(user, It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

            var result = await _sut.RefreshTokenAsync(request, CancellationToken.None);

            result.Should().NotBeNull();
            result!.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();

            _repositoryMock.Verify(
                x => x.SaveRefreshToken(
                    user,
                    It.Is<string>(token => !string.IsNullOrEmpty(token)),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task RefreshTokenAsync_InvalidRefreshToken()
        {
            var request = new RefreshTokenDto
            {
                UserId = 1,
                RefreshToken = "refreshtoken"
            };

            var user = new User
            {
                Id = request.UserId,
                RefreshToken = "diff_refreshtoken",
                RefreshTokenExpiryTime = DateTime.Now.AddDays(1)
            };

            _repositoryMock.Setup(x => x.GetAsync(request.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var result = await _sut.RefreshTokenAsync(request, CancellationToken.None);

            result.Should().BeNull();

            _repositoryMock.Verify(
                x => x.SaveRefreshToken(
                    user,
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task RefreshTokenAsync_ExpiredRefreshToken()
        {
            var request = new RefreshTokenDto
            {
                UserId = 1,
                RefreshToken = "refreshtoken"
            };

            var user = new User
            {
                Id = request.UserId,
                RefreshToken = request.RefreshToken,
                RefreshTokenExpiryTime = DateTime.Now.AddDays(-1)
            };

            _repositoryMock.Setup(x => x.GetAsync(request.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var result = await _sut.RefreshTokenAsync(request, CancellationToken.None);

            result.Should().BeNull();

            _repositoryMock.Verify(
                x => x.SaveRefreshToken(
                    user,
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task RefreshTokenAsync_UserNotExists()
        {
            var request = new RefreshTokenDto
            {
                UserId = 1,
                RefreshToken = "refreshtoken"
            };

            _repositoryMock.Setup(x => x.GetAsync(request.UserId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            var result = await _sut.RefreshTokenAsync(request, CancellationToken.None);

            result.Should().BeNull();

            _repositoryMock.Verify(
                x => x.SaveRefreshToken(
                    It.IsAny<User>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
