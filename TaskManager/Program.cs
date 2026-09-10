using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Text;
using TaskManager.CommonModels;
using TaskManager.DB;
using TaskManager.Middleware;
using TaskManager.Services;
using TaskManager.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override(
        "Microsoft.EntityFrameworkCore",
        LogEventLevel.Warning)
    .Enrich.FromLogContext() // Required to pull properties from LogContext
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] [TraceId: {CorrelationId}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Services.AddOptions<JwtOptions>().BindConfiguration("Jwt");

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
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
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
builder.Services.AddAuthorization(opts => {

    opts.AddPolicy("CanManageTasks", policy =>
    {
        policy.RequireAuthenticatedUser();

        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") ||
            context.User.HasClaim("Position", "Manager"));
    });
});

builder.Services.AddScoped<TaskRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ILoginService, LoginService>();

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

// Middlewares
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    // Customize the message template (Default: "HTTP {Method} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms")
    options.MessageTemplate = "Handled {RequestMethod} {RequestPath} in {Elapsed:0.0000} ms; Status: {StatusCode}";

    // Emit a debug-level log instead of information for successful requests
    options.GetLevel = (httpContext, elapsed, ex) =>
        ex != null ? Serilog.Events.LogEventLevel.Error : Serilog.Events.LogEventLevel.Information;

    // Attach custom properties from the HttpContext to the structured log
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("Host", httpContext.Request.Host.Value);
        diagnosticContext.Set("Protocol", httpContext.Request.Protocol);
    };
});
app.UseMiddleware<ExceptionHandler>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
