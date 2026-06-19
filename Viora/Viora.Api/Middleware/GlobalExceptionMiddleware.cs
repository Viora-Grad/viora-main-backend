using Viora.Application.Abstractions.Exceptions;

namespace Viora.Api.Middleware;

public class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected, not an error, don't log as one.
            return;
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, clientMessage, isExpected) = MapException(ex);

        if (isExpected)
        {
            logger.LogWarning(ex,
                "Handled {ExceptionType} for {Method} {Path}: {Message}",
                ex.GetType().Name, context.Request.Method, context.Request.Path, ex.Message);
        }
        else
        {
            logger.LogError(ex,
                "Unhandled exception for {Method} {Path} (TraceId: {TraceId})",
                context.Request.Method, context.Request.Path, context.TraceIdentifier);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            Error = clientMessage,
            TraceId = context.TraceIdentifier,
        });
    }

    private static (int StatusCode, string ClientMessage, bool IsExpected) MapException(Exception ex)
        => ex switch
        {
            BadRequestException => (StatusCodes.Status400BadRequest, ex.Message, true),
            NotFoundException => (StatusCodes.Status404NotFound, ex.Message, true),
            ConflictException => (StatusCodes.Status409Conflict, ex.Message, true),
            ConcurrencyException => (StatusCodes.Status409Conflict, "The resource was modified by another request. Please retry.", true),
            QuotaExceededException => (StatusCodes.Status405MethodNotAllowed, "The Quota is over please try again later.", true),
            UnallowedMediaException => (StatusCodes.Status415UnsupportedMediaType, "Media sent is not supported by the endpoint", true),
            _ => (StatusCodes.Status500InternalServerError, "An error occurred. Please try again later.", false),
        };
}