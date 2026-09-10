using Serilog.Context;

namespace TaskManager.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeaderKey = "X-Correlation-Id";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Get or generate the correlation ID
            if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderKey, out var correlationId))
            {
                correlationId = context.TraceIdentifier; // Fallback to built-in ASP.NET Trace ID
            }

            // 2. Add the correlation ID to the response header (optional, but good practice)
            context.Response.Headers[CorrelationIdHeaderKey] = correlationId;

            // 3. Push property to Serilog's LogContext
            // The key "CorrelationId" must match the token used in the outputTemplate exactly
            using (LogContext.PushProperty("CorrelationId", correlationId.ToString()))
            {
                await _next(context);
            }
        }
    }
}
