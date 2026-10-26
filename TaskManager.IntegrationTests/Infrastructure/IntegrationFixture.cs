using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.DB;
using Testcontainers.PostgreSql;

namespace TaskManager.IntegrationTests.Infrastructure
{
    public class IntegrationFixture : IAsyncLifetime
    {
        public PostgreSqlContainer Container { get; }
        public CustomWebApplicationFactory Factory { get; private set; } = null!;

        public IntegrationFixture()
        {
            Container = new PostgreSqlBuilder("postgres:16")
                .WithDatabase("task_manager_test")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();
        }

        public async Task InitializeAsync()
        {
            await Container.StartAsync();

            Factory = new CustomWebApplicationFactory(
                Container.GetConnectionString());

            using var scope = Factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationContext>();

            var services = scope.ServiceProvider;

            await db.Database.MigrateAsync();

            await TestDataSeeder.SeedAsync(services);
        }

        public async Task DisposeAsync()
        {
            await Factory.DisposeAsync();
            await Container.DisposeAsync();
        }
    }
}
