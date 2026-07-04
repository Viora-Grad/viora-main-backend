using Viora.Domain.Archives;
using Viora.Domain.Archives.Internals;

namespace Viora.Application.Archives.Shared;

public sealed record TemplateVersionResponse(
    Guid Id,
    Guid TemplateId,
    int Version,
    bool IsPublished,
    IReadOnlyCollection<TemplateVersionFieldResponse> Fields,
    DateTime CreatedAt);

public sealed record TemplateVersionFieldResponse(
    Guid Id,
    string Name,
    string Label,
    FieldType Type,
    bool Required,
    int Order,
    FieldValidation Validation,
    FieldLayout Layout);
