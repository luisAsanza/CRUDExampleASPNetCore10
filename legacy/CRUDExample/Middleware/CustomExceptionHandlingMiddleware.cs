using Microsoft.Extensions.Logging;

namespace CRUDExample.Middleware
{
    public class CustomExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionHandlingMiddleware> _logger;

        public CustomExceptionHandlingMiddleware(RequestDelegate next,
            ILogger<CustomExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "CUSTOM EXCEPTION MIDDLEWARE: An unhandled exception has occurred while processing the request.");

                if(ex.InnerException != null)
                {
                    _logger.LogError(ex, "CUSTOM EXCEPTION MIDDLEWARE: Inner Exception");
                }

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Error - Custom Middleware");
            }
        }
    }

    public static class CustomExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomExceptionHandlingMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CustomExceptionHandlingMiddleware>();
        }
    }
}
