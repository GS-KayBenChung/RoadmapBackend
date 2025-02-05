using System.Net;
using System.Text.Json;
using FluentValidation;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exception switch
        {
            ValidationException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var errorResponse = new
        {
            statusCode = context.Response.StatusCode,
            message = exception.Message,
            details = exception.InnerException?.Message
        };

        if (exception is not ValidationException)
        {
            _logger.LogError(exception, "Error: {Message}", exception.Message);
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }

}
