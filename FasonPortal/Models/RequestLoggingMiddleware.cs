using System.Diagnostics;

namespace FasonPortal.Models
{
    // controller düzeyinde her istekden önce bu çalışır. 
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Debug.WriteLine($"Request Method: {context.Request.Method}");
            Debug.WriteLine($"Request Path: {context.Request.Path}");

            // Call the next delegate/middleware in the pipeline
            await _next(context);

            // metottdan sonra bişeyler
        }
    }
}
