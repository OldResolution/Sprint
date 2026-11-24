using System.Text.Json;
using HotelMS.Auth.Exceptions;

namespace HotelMS.Auth.Exceptions;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
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

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        var traceId = context.TraceIdentifier;

        int statusCode = 500;
        string message = "An unexpected error occurred while processing the request.";
        List<string>? errors = null;

        if (ex is AppException appEx)
        {
            statusCode = appEx.StatusCode;
            message = appEx.Message;
            errors = appEx.Errors;
            _logger.LogWarning("Handled AppException [{StatusCode}]: {Message}", statusCode, message);
        }
        else
        {
            _logger.LogError(ex, "Unhandled Exception: {Message}", ex.Message);
        }

        context.Response.StatusCode = statusCode;

        var payload = new
        {
            statusCode,
            message,
            errors,
            traceId
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));
    }
}
