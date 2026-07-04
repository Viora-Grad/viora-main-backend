namespace Viora.Api.Controllers.Archives;

public sealed record CreateFolderRequest(
    Guid? ParentFolderId,
    string Name,
    string? Description,
    string? Type,
    int Order
);
