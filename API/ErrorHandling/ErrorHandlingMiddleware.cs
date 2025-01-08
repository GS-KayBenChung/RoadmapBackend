using Newtonsoft.Json;
using System.Net;

namespace API.ErrorHandling
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (ConflictException ex)
            {
                _logger.LogError(ex, "Conflict Error occurred");
                httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                await httpContext.Response.WriteAsJsonAsync(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await httpContext.Response.WriteAsJsonAsync(new { message = "An unexpected error occurred" });
            }
        }

        //private Task HandleExceptionAsync(HttpContext context, Exception exception)
        //{
        //    context.Response.ContentType = "application/json";
        //    var statusCode = HttpStatusCode.InternalServerError;
        //    var message = "An unexpected error occurred.";

        //    if (exception is ApplicationException appEx)
        //    {
        //        message = appEx.Message;
        //        statusCode = HttpStatusCode.BadRequest;
        //    }

        //    context.Response.StatusCode = (int)statusCode;

        //    var result = JsonConvert.SerializeObject(new { error = message });
        //    return context.Response.WriteAsync(result);
        //}
    }

}