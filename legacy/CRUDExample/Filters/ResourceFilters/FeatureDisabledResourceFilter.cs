using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ResourceFilters
{
    public class FeatureDisabledResourceFilter : IAsyncResourceFilter
    {
        private readonly ILogger<FeatureDisabledResourceFilter> _logger;

        public FeatureDisabledResourceFilter(ILogger<FeatureDisabledResourceFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            _logger.LogInformation("FeatureDisabledResourceFilter.OnResourceExecutionAsync");

            var randomNumber = new Random().Next(1, 3);

            if(randomNumber == 1)
            {
                _logger.LogWarning("Feature is disabled. Returning 503 Service Unavailable");
                context.HttpContext.Response.StatusCode = 503; // Service Unavailable
                await context.HttpContext.Response.WriteAsync("This feature is currently disabled. Please try again later.");
                //context.Result = new StatusCodeResult(404);
                return;
            }

            _logger.LogDebug("Feature is enabled. Proceeding to next resource");
            var executedContext = await next();

            if (executedContext.Canceled)
            {
                //Do something if filter short-circuited the request
            }
        }
    }
}
