using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Viora.Api.OpenApi;

/// <summary>
/// Startup MVC convention that infers each action's MediatR response type (via
/// <see cref="CommandResponseTypeResolver"/>) and injects <see cref="ProducesResponseTypeAttribute"/>
/// metadata, so native OpenAPI schema generation can produce response bodies/samples without
/// per-endpoint decorators. Also annotates the common error responses.
/// </summary>
internal sealed class ProducesResponseConvention(bool addErrorResponses = true) : IApplicationModelConvention
{
    private static readonly int[] ErrorCodes =
    [
        StatusCodes.Status400BadRequest,
        StatusCodes.Status401Unauthorized,
        StatusCodes.Status403Forbidden,
        StatusCodes.Status404NotFound,
        StatusCodes.Status408RequestTimeout,
        StatusCodes.Status409Conflict,
        StatusCodes.Status422UnprocessableEntity,
        StatusCodes.Status500InternalServerError,
        StatusCodes.Status502BadGateway,
    ];

    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            foreach (var action in controller.Actions)
            {
                try
                {
                    ApplyToAction(action);
                }
                catch
                {
                    // Never break startup over a documentation nicety.
                }
            }
        }
    }

    private void ApplyToAction(ActionModel action)
    {
        var declaredCodes = CollectDeclaredStatusCodes(action);

        // Only infer the success response if the developer hasn't already declared a 2xx.
        if (!declaredCodes.Any(code => code is >= 200 and <= 299))
        {
            var resolved = CommandResponseTypeResolver.Resolve(action.ActionMethod);
            if (resolved is { } response)
            {
                if (response.IsNoBody)
                {
                    action.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent));
                    declaredCodes.Add(StatusCodes.Status204NoContent);
                }
                else if (response.ResponseType is not null)
                {
                    action.Filters.Add(new ProducesResponseTypeAttribute(response.ResponseType, StatusCodes.Status200OK));
                    declaredCodes.Add(StatusCodes.Status200OK);
                }
            }
        }

        if (!addErrorResponses)
            return;

        foreach (var code in ErrorCodes)
        {
            if (declaredCodes.Add(code))
                action.Filters.Add(new ProducesResponseTypeAttribute(typeof(ProblemDetails), code));
        }
    }

    private static HashSet<int> CollectDeclaredStatusCodes(ActionModel action)
    {
        var codes = new HashSet<int>();

        foreach (var metadata in action.Attributes.OfType<IApiResponseMetadataProvider>())
            codes.Add(metadata.StatusCode);

        foreach (var metadata in action.Filters.OfType<IApiResponseMetadataProvider>())
            codes.Add(metadata.StatusCode);

        return codes;
    }
}
