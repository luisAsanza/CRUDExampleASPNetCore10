using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ResultFilters
{
    public class PersonsListResultFilter : IAsyncResultFilter
    {
        private readonly ILogger<PersonsListResultFilter> _logger;

        public PersonsListResultFilter(ILogger<PersonsListResultFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            _logger.LogInformation("PersonsListResultFilter.OnResultExecutionAsync - Before result execution");
            _logger.LogDebug("Result type: {ResultType}", context.Result?.GetType().Name);

            context.HttpContext.Response.Headers["Last-Modified"] = DateTime.Now.ToString("R");

            await next();

            _logger.LogInformation("PersonsListResultFilter.OnResultExecutionAsync - After result execution");            
        }
    }
}
