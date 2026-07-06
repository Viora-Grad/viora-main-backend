namespace Viora.Api.Controllers.Archives;

public sealed record UpdateFolderRequest(
    string Name,
    string? Description,
    int Order
);
