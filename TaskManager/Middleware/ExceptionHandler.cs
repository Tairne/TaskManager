using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using TaskManager.Services;
using TaskManager.Services.Interfaces;

namespace TaskManager.Middleware
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;

        public ExceptionHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IApplicationMetrics metrics)
        {
            var stopwatch = Stopwatch.StartNew();
            metrics.RequestStarted();
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                metrics.RequestFailed();
                await HandleExceptionAsync(context, ex);
            }
            finally
            {
                stopwatch.Stop();
                metrics.RequestCompleted(stopwatch.Elapsed);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // Customize status code based on exception type
            context.Response.StatusCode = exception switch
            {
                ArgumentException => (int)HttpStatusCode.BadRequest,
                BadHttpRequestException => (int)HttpStatusCode.BadRequest,
                RequestLimitExceeded => (int)HttpStatusCode.BadRequest,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                DuplicateLoginException => (int)HttpStatusCode.Conflict,
                InvalidOperationException => (int)HttpStatusCode.Conflict,
                DbUpdateConcurrencyException => (int)HttpStatusCode.Conflict,
                FluentValidation.ValidationException => (int)HttpStatusCode.UnprocessableEntity,
                TimeoutException => (int)HttpStatusCode.GatewayTimeout,
                TaskCanceledException => (int)HttpStatusCode.GatewayTimeout,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = exception.Message // Return the exception message
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
