using Microsoft.AspNetCore.Builder;
using System.Text;

namespace CRUDExample.Middleware
{
    public class CustomExceptionLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionLoggerMiddleware> _logger;

        public CustomExceptionLoggerMiddleware(RequestDelegate next, ILogger<CustomExceptionLoggerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1 Enable Buffering to allow read and rewind body
            context.Request.EnableBuffering();

            // 2 Set position to read from beginning
            context.Request.Body.Position = 0;

            // 3 Create scoped StreamReader and leave Body open for subsequent middlewares
            using (var reader = new StreamReader(
                context.Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                _logger.LogInformation("CustomExceptionLoggerMiddleware - Method = {method}, ContentLength = {len}, Body: {body}", context.Request.Method, context.Request.ContentLength, body);
            }

            // 4 Reset position for next middleware
            context.Request.Body.Position = 0;

            await _next(context);
        }
    }

    public static class CustomExceptionLoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomExceptionLoggerMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CustomExceptionLoggerMiddleware>();
        }
    }
}
