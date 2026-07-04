namespace Viora.Api.Controllers.Archives;

public sealed record UpdateTemplateRequest(
    string Name,
    string? Description
);
