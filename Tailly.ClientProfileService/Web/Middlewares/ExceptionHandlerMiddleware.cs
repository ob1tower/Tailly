using System.Security.Authentication;
using Tailly.ClientProfileService.Application.Dtos.Common;

namespace Tailly.ClientProfileService.Web.Middlewares;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next,
                                      ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next.Invoke(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");
            await HandleExceptionMessageAsync(context, ex);
        }
    }

    private static async Task HandleExceptionMessageAsync(HttpContext context, Exception exception)
    {
        ExceptionResponse response = exception switch
        {
            KeyNotFoundException _ => new()
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "The requested resource could not be found.",
            },
            InvalidOperationException _ => new()
            {
                StatusCode = StatusCodes.Status409Conflict,
                Message = "A conflict occurred while processing your request.",
            },
            AuthenticationException _ => new()
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Authentication failed. Please check your credentials and try again.",
            },
            _ => new()
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Internal server error.",
            }
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.StatusCode;

        await context.Response.WriteAsJsonAsync(response);
    }
}