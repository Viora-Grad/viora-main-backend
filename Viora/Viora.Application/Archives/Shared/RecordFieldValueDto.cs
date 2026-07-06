namespace Viora.Application.Archives.Shared;

public sealed record RecordFieldValueDto(
    string FieldName,
    object? Value
);
