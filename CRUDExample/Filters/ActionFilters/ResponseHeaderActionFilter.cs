using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging.Abstractions;

namespace CRUDExample.Filters.ActionFilters
{
    public class ResponseHeaderFilterFactoryAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => false;

        public ResponseHeaderFilterFactoryAttribute()
        {
            
        }

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            //Return filter object
            var filter = new ResponseHeaderActionFilter(NullLogger<ResponseHeaderActionFilter>.Instance,
                "key-123", "value-123", 0);
            return filter;
        }
    }

    public class ResponseHeaderActionFilter : IAsyncActionFilter, IOrderedFilter
    {
        private readonly ILogger<ResponseHeaderActionFilter> _logger;
        private readonly string _key;
        private readonly string _value;

        public int Order { get; set; }

        public ResponseHeaderActionFilter(ILogger<ResponseHeaderActionFilter> logger, string key, string value, int order)
        {
            _logger = logger;
            _key = key;
            _value = value;
            Order = order;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            _logger.LogInformation("{FilterName}.{MethodName} method BEFORE", nameof(ResponseHeaderActionFilter), nameof(OnActionExecutionAsync));

            context.HttpContext.Response.Headers[_key] = _value;

            // Always call the next delegate/middleware in the pipeline (It calls subsequent action filters and action method)
            await next();

            _logger.LogInformation("{FilterName}.{MethodName} method AFTER", nameof(ResponseHeaderActionFilter), nameof(OnActionExecutionAsync));
        }        

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogInformation("{FilterName}.{MethodName} method", nameof(ResponseHeaderActionFilter), nameof(OnActionExecuted));

            context.HttpContext.Response.Headers[_key] = _value;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogInformation("{FilterName}.{MethodName} method", nameof(ResponseHeaderActionFilter), nameof(OnActionExecuting));
        }

    }
}
