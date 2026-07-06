namespace Viora.Application.Archives.Shared;

public sealed record FieldValidationDto(
    bool Required,
    int? MinLength,
    int? MaxLength,
    decimal? Min,
    decimal? Max,
    string? Regex
);
