using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace CRUDExample.Pages
{
    public class ErrorModel : PageModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // Add this property
        public int StatusCode { get; set; }

        public void OnGet(int? code)
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            // Capture the code, default to 500 if null
            StatusCode = code ?? 500;
        }
    }
}
