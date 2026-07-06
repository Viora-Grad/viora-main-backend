namespace Viora.Domain.Archives.Internals;

public record FieldValidation(
    bool Required,
    int? MinLength,
    int? MaxLength,
    decimal? Min,
    decimal? Max,
    string? Regex
    );

