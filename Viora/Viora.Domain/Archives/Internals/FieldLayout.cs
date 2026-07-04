namespace Viora.Domain.Archives.Internals;

public record FieldLayout
(
    int Column,
    int Order,
    string? Tab,
    int Width
);