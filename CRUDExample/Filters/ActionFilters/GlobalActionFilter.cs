using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ActionFilters
{
    /// <summary>
    /// Global action filter that applies to all actions in the application.
    /// Logs action execution lifecycle events.
    /// </summary>
    public class GlobalActionFilter : IActionFilter
    {
        private readonly ILogger<GlobalActionFilter> _logger;

        public GlobalActionFilter(ILogger<GlobalActionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogInformation("GlobalActionFilter.OnActionExecuting - {Controller}.{Action}",
                context.RouteData.Values["controller"],
                context.RouteData.Values["action"]);

            _logger.LogDebug("Action Arguments: {ArgumentCount} arguments", context.ActionArguments.Count);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogInformation("GlobalActionFilter.OnActionExecuted - {Controller}.{Action}",
                context.RouteData.Values["controller"],
                context.RouteData.Values["action"]);

            if (context.Exception != null)
            {
                _logger.LogError(context.Exception, "An exception occurred in action");
            }
            else
            {
                _logger.LogDebug("Action executed successfully. Result type: {ResultType}",
                    context.Result?.GetType().Name ?? "null");
            }
        }
    }
}
