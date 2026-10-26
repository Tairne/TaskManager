using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.DB;
using TaskManager.DB.DataModels;
using TaskManager.Services.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace TaskManager.IntegrationTests.Infrastructure
{
    public static class TestDataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<ApplicationContext>();

            var passwordService =
                services.GetRequiredService<IPasswordService>();

            var passwordHash = passwordService.Hash("Test123!");

            db.AddRange(
                new User
                {
                    Login = "test-user",
                    Password = passwordHash,
                    UserName = "Test User",
                    Role = "User",
                    Position = "Worker"
                },
                new User
                {
                    Login = "assigned-user",
                    Password = passwordHash,
                    UserName = "Assigned User",
                    Role = "User",
                    Position = "Worker"
                },
                new User
                {
                    Login = "test-manager",
                    Password = passwordHash,
                    UserName = "Test Manager",
                    Role = "User",
                    Position = "Manager"
                },
                new User
                {
                    Login = "test-admin",
                    Password = passwordHash,
                    UserName = "Test Admin",
                    Role = "Admin",
                    Position = "Admin"
                });

            await db.SaveChangesAsync();
        }
    }
}
