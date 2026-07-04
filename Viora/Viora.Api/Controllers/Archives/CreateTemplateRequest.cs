namespace Viora.Api.Controllers.Archives;

public sealed record CreateTemplateRequest(
    Guid FolderId,
    string Name,
    string? Description
);
