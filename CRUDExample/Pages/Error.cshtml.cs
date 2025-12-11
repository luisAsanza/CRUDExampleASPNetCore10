using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRUDExample.Pages
{
    public class ErrorModel : PageModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // Add this property
        public int StatusCode { get; set; }

        private readonly ILogger<ErrorModel> _logger;

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(int? code)
        {
            RequestId = HttpContext.TraceIdentifier;

            // Capture the code, default to 500 if null
            StatusCode = code ?? 500;

            //Log the error with status code
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if(exceptionFeature != null)
            {
                var exception = exceptionFeature.Error;
                var path = exceptionFeature.Path;
                _logger.LogError(exception, $"Error occurred on path: {path} with RequestId: {RequestId}");
            }
        }
    }
}
