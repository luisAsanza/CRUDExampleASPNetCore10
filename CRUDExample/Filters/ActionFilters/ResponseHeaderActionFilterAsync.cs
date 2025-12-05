using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ActionFilters
{
    public class ResponseHeaderActionFilterAsync : IAsyncActionFilter, IOrderedFilter
    {
        private readonly ILogger<ResponseHeaderActionFilterAsync> _logger;
        private readonly string _key;
        private readonly string _value;

        public int Order { get; set; }

        public ResponseHeaderActionFilterAsync(ILogger<ResponseHeaderActionFilterAsync> logger, string key, string value, int order)
        {
            _logger = logger;
            _key = key;
            _value = value;
            Order = order;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            _logger.LogInformation("{FilterName}.{MethodName} method BEFORE", nameof(ResponseHeaderActionFilterAsync), nameof(OnActionExecutionAsync));

            context.HttpContext.Response.Headers[_key] = _value;

            // Always call the next delegate/middleware in the pipeline (It calls subsequent action filters and action method)
            await next();

            _logger.LogInformation("{FilterName}.{MethodName} method AFTER", nameof(ResponseHeaderActionFilterAsync), nameof(OnActionExecutionAsync));
        }        

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogInformation("{FilterName}.{MethodName} method", nameof(ResponseHeaderActionFilterAsync), nameof(OnActionExecuted));

            context.HttpContext.Response.Headers[_key] = _value;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogInformation("{FilterName}.{MethodName} method", nameof(ResponseHeaderActionFilterAsync), nameof(OnActionExecuting));
        }

    }
}
