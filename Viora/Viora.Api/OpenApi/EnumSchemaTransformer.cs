using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Viora.Api.OpenApi;

internal sealed class EnumSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        var enumType = Nullable.GetUnderlyingType(type) ?? type;

        if (!enumType.IsEnum)
            return Task.CompletedTask;

        var names = Enum.GetNames(enumType);
        var values = Enum.GetValues(enumType).Cast<object>()
            .Select(v => Convert.ToInt64(v))
            .ToArray();

        schema.Description = string.Join(" | ", names.Zip(values, (name, value) => $"{value} = {name}"));

        return Task.CompletedTask;
    }
}
