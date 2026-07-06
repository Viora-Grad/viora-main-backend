namespace Viora.Domain.Archives.Internals;

public sealed record RecordFieldValue
(
    Guid FieldId,
    string FieldName,
    FieldType FieldType,
    object? Value
);