using System.Text.Json;

namespace Viora.Application.Archives.Shared;

internal static class JsonValueConverter
{
    public static object? ToObject(object? value)
    {
        if (value is JsonElement jsonElement)
            return ConvertJsonElement(jsonElement);

        return value;
    }

    private static object? ConvertJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return element.GetString();

            case JsonValueKind.Number:
                if (element.TryGetInt32(out var intVal))
                    return intVal;
                if (element.TryGetInt64(out var longVal))
                    return longVal;
                return element.GetDouble();

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;

            case JsonValueKind.Array:
                return element.EnumerateArray().Select(ConvertJsonElement).ToList();

            case JsonValueKind.Object:
                return element.EnumerateObject()
                    .ToDictionary(p => p.Name, p => ConvertJsonElement(p.Value));

            default:
                return null;
        }
    }
}
