namespace Viora.Application.Archives.Shared;

public sealed record FieldLayoutDto(
    int Column,
    int Order,
    string? Tab,
    int Width
);
