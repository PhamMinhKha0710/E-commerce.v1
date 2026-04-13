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
        
        // Mapping HTTP Codes dựa theo loại Exception
        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                message = "Dữ liệu đầu vào không hợp lệ.";
                errors = validationException.Errors
                    .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                    .ToDictionary(g => g.Key, g => g.ToArray());
                break;
                
            case E_commerce.v1.Domain.Exceptions.BadRequestException badRequestEx:
                statusCode = HttpStatusCode.BadRequest;
                message = badRequestEx.Message;
                break;

            case E_commerce.v1.Domain.Exceptions.NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                message = notFoundEx.Message;
                break;
                
            default:
                message = "An internal server error occurred.";
                break;
        }

        context.Response.StatusCode = (int)statusCode;
        var result = JsonSerializer.Serialize(new { StatusCode = statusCode, Message = message, Errors = errors });
        return context.Response.WriteAsync(result);
    }
}
