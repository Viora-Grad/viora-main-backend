using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Viora.Api.OpenApi;

/// <summary>
/// Declares a JWT Bearer security scheme on the OpenAPI document and requires it globally.
/// This is what lets Scalar (and any OpenAPI client) show an "Authentication" section where the
/// token is entered once and automatically attached to every request, instead of adding the
/// Authorization header per call.
/// </summary>
internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    private const string SchemeId = "Bearer";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes[SchemeId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Paste the JWT access token. It is sent as: Authorization: Bearer {token}",
        };

        // Require the scheme globally so every operation carries the token by default.
        var requirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(SchemeId, document)] = [],
        };

        document.Security ??= [];
        document.Security.Add(requirement);

        return Task.CompletedTask;
    }
}
