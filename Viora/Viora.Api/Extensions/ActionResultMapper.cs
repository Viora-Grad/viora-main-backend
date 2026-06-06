using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Viora.Domain.Abstractions;

namespace Viora.Api.Extensions;

/// <summary>
/// static factory that returns actionresult based on the result passed
/// </summary>
internal static class ActionResultMapper
{
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new NoContentResult();

        return BuildProblemDetails(result.Error);
    }

    public static IActionResult ToActionResult<T>(this Result<T> result, string? createdAtAction = null, object? routeValues = null)
    {
        if (result.IsSuccess)
        {
            if (createdAtAction != null)
                return new CreatedAtActionResult(createdAtAction, null, routeValues, result.Value);

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

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Name,
            Detail = error.Description
        };

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }
}
