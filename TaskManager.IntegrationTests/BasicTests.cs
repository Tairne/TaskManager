using Microsoft.AspNetCore.Mvc.Testing;
using TaskManager.IntegrationTests.Infrastructure;

namespace TaskManager.IntegrationTests
{
    public class BasicTests : IClassFixture<IntegrationFixture>
    {
        private readonly IntegrationFixture _fixture;

        public BasicTests(IntegrationFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Get_Swagger_ReturnsSuccess()
        {
            var client = _fixture.Factory.CreateClient();

            var response = await client.GetAsync("/swagger/index.html");

            response.EnsureSuccessStatusCode();
        }
    }
}