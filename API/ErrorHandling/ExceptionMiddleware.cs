using FluentValidation;
using Serilog;
using System.Net;
using System.Text.Json;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var traceId = context.TraceIdentifier;

        //if (exception is ValidationException validationException)
        //{
        //    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //    var errors = validationException.Errors
        //        .GroupBy(e => "Validation")
        //        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        //    Log.Warning("[{TraceId}] Validation failed: {Errors}", traceId, string.Join(", ", errors["Validation"]));

        //    var response = new
        //    {
        //        message = "Validation failed",
        //        errors
        //    };

        //    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        //    return;
        //}

        if (exception is ValidationException validationException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName) 
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            Log.Warning("[{TraceId}] Validation failed: {Errors}", traceId, string.Join(", ", errors.SelectMany(kvp => kvp.Value)));

            var response = new
            {
                message = "Validation failed",
                errors, 
                traceId
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            return;
        }

        if (exception is BadHttpRequestException || exception is ArgumentNullException || exception is FormatException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            Log.Warning("Validation failed: Invalid request format or data type.");

            var response = new
            {
                message = "Validation failed",
                errors = new { Validation = new[] { "Invalid request format or data type." } }
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            return;
        }

        if (exception is JsonException jsonException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            var errorMessage = "Invalid JSON format or unknown fields detected.";
            Log.Warning("[{TraceId}] {ErrorMessage}: {ExceptionMessage}", traceId, errorMessage, jsonException.Message);

            var response = new
            {
                message = "Validation failed",
                errors = new { Validation = new[] { errorMessage } }
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            return;
        }


        Log.Error("[{TraceId}] An unexpected error occurred: {Message}", traceId, exception.Message);

        var errorResponse = new
        {
            message = "An unexpected error occurred",
            errors = new { Details = new[] { exception.Message } },
            traceId
        };

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}