using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using TaskManager.DB;
using TaskManager.DB.DataModels;
using TaskManager.DB.DTO;
using TaskManager.IntegrationTests.Infrastructure;
using TaskManager.Services.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace TaskManager.IntegrationTests
{
    public class AccountApiTests : IntegrationTestBase, IClassFixture<IntegrationFixture>
    {
        public AccountApiTests(IntegrationFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task CreateAccount_AccountCreated()
        {
            // Arrange
            using var client = Factory.CreateClient();

            const string password = "password";

            var account = new CreateUserDto
            {
                Login = "created_user@test.com",
                Password = password,
                Position = "Worker",
                UserName = "username"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/accounts", account);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var createdAccount = await response.Content.ReadFromJsonAsync<User>();

            Assert.NotNull(createdAccount);

            using var scope = Factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationContext>();

            var passwordService = scope.ServiceProvider
                .GetRequiredService<IPasswordService>();

            var createdUser = await db.Users
                .SingleAsync(x => x.Login == account.Login);

            Assert.NotEqual(password, createdUser.Password);

            Assert.True(
                passwordService.Verify(password, createdUser.Password));
        }

        [Fact]
        public async Task CreateAccount_ValidationError()
        {
            using var client = Factory.CreateClient();

            var account = new CreateUserDto
            {
                Login = "invalid_user",
                Password = "password",
                Position = "Worker",
                UserName = "username"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/accounts", account);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task CreateAccount_UserAlreadyExist()
        {
            using var client = Factory.CreateClient();

            var account = new CreateUserDto
            {
                Login = "new_user@test.com",
                Password = "password",
                Position = "Worker",
                UserName = "username"
            };

            // Act
            var firstResponse = await client.PostAsJsonAsync(
                "/api/accounts", account);

            var secondResponse = await client.PostAsJsonAsync(
                "/api/accounts", account);

            // Assert
            Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        }

        [Fact]
        public async Task UpdateAccount_AccountUpdated()
        {
            var client = await CreateAuthenticatedClientAsync(
                "test-admin",
                "Test123!");

            var created_account = new CreateUserDto
            {
                Login = "updated_user@test.com",
                Password = "password",
                Position = "Worker",
                UserName = "username"
            };

            var updateRequest = new UpdateUserDto
            {
                Login = "updated_user@test.com",
                Position = "New position",
                UserName = "new username"
            };

            var account = await CreateAccountAsync(client, created_account);

            var updateResponse = await client.PutAsJsonAsync($"/api/accounts/{account.Id}", updateRequest);

            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

            using var scope = Factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationContext>();

            var updatedAccount = await db.Set<User>().SingleAsync(x => x.Id == account.Id);

            Assert.Equal("New position", updatedAccount.Position);
            Assert.Equal("new username", updatedAccount.UserName);
        }

        [Fact]
        public async Task UpdateAccount_AccessForbiden()
        {
            var client = await CreateAuthenticatedClientAsync(
                "test-manager",
                "Test123!");

            var updateRequest = new UpdateUserDto
            {
                Login = "updated_user@test.com",
                Position = "New position",
                UserName = "new username"
            };

            var updateResponse = await client.PutAsJsonAsync($"/api/accounts/1", updateRequest);

            Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);
        }

        [Fact]
        public async Task UpdateAccount_AccountNotExists()
        {
            var client = await CreateAuthenticatedClientAsync(
                "test-admin",
                "Test123!");

            var updateRequest = new UpdateUserDto
            {
                Login = "updated_user@test.com",
                Position = "New position",
                UserName = "new username"
            };

            var updateResponse = await client.PutAsJsonAsync($"/api/accounts/{int.MaxValue}", updateRequest);

            Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
        }

        [Fact]
        public async Task ChangePassword_PasswordChanged()
        {
            string new_password = "NewTest123!";
            string login = "assigned-user";
            var client = await CreateAuthenticatedClientAsync(
                login,
                "Test123!");

            var request = new ChangePasswordDto
            {
                CurrentPassword = "Test123!",
                NewPassword = new_password
            };

            var response = await client.PostAsJsonAsync(
                "/api/accounts/cpwd", request);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            using var scope = Factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationContext>();

            var passwordService = scope.ServiceProvider
                .GetRequiredService<IPasswordService>();

            var currentUser = await db.Users
                .SingleAsync(x => x.Login == login);

            Assert.True(
                passwordService.Verify(new_password, currentUser.Password));
        }

        [Fact]
        public async Task ChangePassword_WrongPassword()
        {
            var client = await CreateAuthenticatedClientAsync(
                "test-manager",
                "Test123!");

            var request = new ChangePasswordDto
            {
                CurrentPassword = "Test123!1111",
                NewPassword = "newpassword"
            };

            var response = await client.PostAsJsonAsync(
                "/api/accounts/cpwd", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ChangePassword_InvalidPassword()
        {
            var client = await CreateAuthenticatedClientAsync(
                "test-manager",
                "Test123!");

            var request = new ChangePasswordDto
            {
                CurrentPassword = "Test123!",
                NewPassword = string.Empty
            };

            var response = await client.PostAsJsonAsync(
                "/api/accounts/cpwd", request);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task GetAccountInfo_ExistedAccountReturns()
        {
            var client = await CreateAuthenticatedClientAsync(
                "test-admin",
                "Test123!");

            var created_account = new CreateUserDto
            {
                Login = "existed_user@test.com",
                Password = "password",
                Position = "Worker",
                UserName = "username"
            };

            var account = await CreateAccountAsync(client, created_account);

            var response = await client.GetAsync(
                $"/api/accounts/{account.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var actualAccount = await response.Content
                .ReadFromJsonAsync<User>();

            Assert.NotNull(actualAccount);
            Assert.Equal(account.Id, actualAccount.Id);
            Assert.Equal(account.Login, actualAccount.Login);
        }

        [Fact]
        public async Task GetAccountInfo_AccessForbidden()
        {
            var client = await CreateAuthenticatedClientAsync(
                "test-manager",
                "Test123!");

            var response = await client.GetAsync(
                $"/api/accounts/1");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetAccountInfo_AccountNotExists()
        {
            var client = await CreateAuthenticatedClientAsync(
                "test-admin",
                "Test123!");

            var response = await client.GetAsync(
                $"/api/accounts/{int.MaxValue}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAll_ReturnsAllUsers()
        {
            var client = await CreateAuthenticatedClientAsync(
                "test-manager",
                "Test123!");

            var accounts = new List<CreateUserDto>
            {
                new CreateUserDto
                {
                    Login = "user1@test.com",
                    Password = "password",
                    Position = "Worker",
                    UserName = "name1",
                },
                new CreateUserDto
                {
                    Login = "user2@test.com",
                    Password = "password",
                    Position = "Worker",
                    UserName = "name2",
                }
            };

            foreach(var account in accounts)
            {
                await client.PostAsJsonAsync("/api/accounts", account);
            }

            var response = await client.GetAsync(
                $"/api/accounts");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var users = await response.Content
                .ReadFromJsonAsync<List<AllUsersDto>>();

            Assert.NotNull(users);
        }
    }
}
