using Microsoft.EntityFrameworkCore;
using TaskManager.DB;
using TaskManager.Services;
using TaskManager.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"),
    npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
    })
    .UseSnakeCaseNamingConvention());
builder.Services.AddScoped<TaskRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITaskService, TaskService>();

builder.Services.AddTransient<IExportService, ExportService>();

builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddSingleton<IApplicationMetrics, ApplicationMetrics>();


var app = builder.Build();

// Migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
