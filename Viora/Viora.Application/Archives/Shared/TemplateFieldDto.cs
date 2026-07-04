using Viora.Domain.Archives;

namespace Viora.Application.Archives.Shared;

public sealed record TemplateFieldDto(
    string Name,
    string Label,
    FieldType Type,
    bool Required,
    int Order,
    FieldValidationDto? Validation,
    FieldLayoutDto? Layout
);
