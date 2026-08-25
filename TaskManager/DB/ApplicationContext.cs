using Microsoft.EntityFrameworkCore;
using TaskManager.DB.DataModels;
using TaskManager.Enums;
using Task = TaskManager.DB.DataModels.Task;

namespace TaskManager.DB
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Task> Tasks => Set<Task>();
        public DbSet<User> Users => Set<User>();
        
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) 
        { 
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Task>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Status).HasDefaultValue(Status.Created);
                entity.Property(e => e.Priority).HasDefaultValue("0");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            });
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Login).IsRequired().HasMaxLength(32);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).HasDefaultValueSql("User");
            });
        }
    }
}
