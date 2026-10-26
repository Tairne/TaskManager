using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using TaskManager.DB.DataModels;
using TaskManager.DB.DTO;

namespace TaskManager.IntegrationTests.Infrastructure
{
    public class IntegrationTestBase
    {
        protected readonly CustomWebApplicationFactory Factory;

        protected IntegrationTestBase(IntegrationFixture fixture)
        {
            Factory = fixture.Factory;
        }

        protected async Task<HttpClient> CreateAuthenticatedClientAsync(
            string login,
            string password)
        {
            var client = Factory.CreateClient();

            var loginRequest = new LoginDto
            {
                Login = login,
                Password = password
            };

            var response = await client.PostAsJsonAsync(
                "/api/auth/login",
                loginRequest);

            response.EnsureSuccessStatusCode();

            var token = await response.Content
                .ReadFromJsonAsync<TokenResponseDto>();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token!.AccessToken);

            return client;
        }

        protected async Task<DB.DataModels.Task> CreateTaskAsync(
            HttpClient client,
            CreateTaskDto request)
        {
            var response = await client.PostAsJsonAsync(
                "/api/tasks",
                request);

            response.EnsureSuccessStatusCode();

            var task = await response.Content
                .ReadFromJsonAsync<DB.DataModels.Task>();

            Assert.NotNull(task);

            return task;
        }

        protected async Task<User> CreateAccountAsync(HttpClient client,
            CreateUserDto request)
        {
            var response = await client.PostAsJsonAsync(
                "/api/accounts",
                request);

            response.EnsureSuccessStatusCode();

            var user = await response.Content
                .ReadFromJsonAsync<User>();

            Assert.NotNull(user);

            return user;
        }
    }
}
