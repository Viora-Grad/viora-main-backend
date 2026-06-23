using Microsoft.AspNetCore.Mvc;
using Serilog;
using Viora.Domain.Abstractions;

namespace Viora.Api.Extensions;

/// <summary>
/// static factory that returns actionresult based on the result passed
/// </summary>
internal static class ActionResultMapper
{
    private static readonly Serilog.ILogger Logger = Log.ForContext(typeof(ActionResultMapper));

    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new NoContentResult();

        return BuildProblemDetails(result.Error);
    }

    public static IActionResult ToActionResult<T>(
        this Result<T> result,
        string? createdAtAction = null,
        Func<T, object>? routeValueFactory = null, // Changed to a factory function
        string? createdAtController = null) // null => current controller; set when the action lives on another controller
    {
        if (result.IsSuccess)
        {
            if (createdAtAction != null && routeValueFactory != null)
            {
                // Safely evaluate the route values using the successful result value
                var routeValues = routeValueFactory(result.Value);
                return new CreatedAtActionResult(createdAtAction, createdAtController, routeValues, result.Value);
            }

            return new OkObjectResult(result.Value);
        }

        return BuildProblemDetails(result.Error);
    }

    private static ObjectResult BuildProblemDetails(Error error)
    {
        var statusCode = error.Category switch
        {
            ErrorCategory.Validation => StatusCodes.Status400BadRequest,
            ErrorCategory.NotFound => StatusCodes.Status404NotFound,
            ErrorCategory.Conflict => StatusCodes.Status409Conflict,
            ErrorCategory.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorCategory.Forbidden => StatusCodes.Status403Forbidden,
            ErrorCategory.Violation => StatusCodes.Status422UnprocessableEntity,
            ErrorCategory.Timeout => StatusCodes.Status408RequestTimeout,
            ErrorCategory.BadGateway => StatusCodes.Status502BadGateway,
            _ => StatusCodes.Status500InternalServerError,
        };

        // Result-based failures never throw, so they bypass the exception middleware's logging.
        // Log them here: server-side faults at Error, expected client errors at Warning.
        if (statusCode >= StatusCodes.Status500InternalServerError)
            Logger.Error("Request failed: {ErrorName} ({Category}) -> {StatusCode}: {ErrorDescription}",
                error.Name, error.Category, statusCode, error.Description);
        else
            Logger.Warning("Request rejected: {ErrorName} ({Category}) -> {StatusCode}: {ErrorDescription}",
                error.Name, error.Category, statusCode, error.Description);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Name,
            Detail = error.Description
        };

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }
}
