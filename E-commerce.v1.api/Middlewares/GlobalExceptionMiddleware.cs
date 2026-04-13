using System.Net;
using System.Text.Json;
using FluentValidation;

namespace E_commerce.v1.api.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An internal server error occurred.";
        object? errors = null;

        // Nếu lỗi là do ValidationBehavior văng ra (Tất cả DTOs sai)
        if (exception is ValidationException validationException)
        {
            statusCode = HttpStatusCode.BadRequest;
            message = "Validation Failed";
            errors = validationException.Errors
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
        }

        context.Response.StatusCode = (int)statusCode;
        var result = JsonSerializer.Serialize(new { StatusCode = statusCode, Message = message, Errors = errors });
        return context.Response.WriteAsync(result);
    }
}
