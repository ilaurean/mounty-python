using System.Text.Json;
using System.Text.Json.Serialization;

namespace Commons.Services;

internal class IntConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert == typeof(int) || typeToConvert == typeof(int?);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        typeToConvert == typeof(int)
            ? new IntConverter()
            : new NullableIntConverter();
}

internal class IntConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                if (reader.TryGetInt32(out int intValue))
                    return intValue;
                if (reader.TryGetDouble(out double doubleValue))
                    return (int)Math.Round(doubleValue);
                break;
            case JsonTokenType.String:
                var str = reader.GetString();
                if (!string.IsNullOrEmpty(str))
                {
                    var cleaned = str.Replace(",", "").Replace(".", "");
                    if (int.TryParse(cleaned, out int result))
                        return result;
                }
                return 0;
            case JsonTokenType.Null:
                return 0;
        }

        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value);
}

internal class NullableIntConverter : JsonConverter<int?>
{
    public override bool HandleNull => true;

    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                if (reader.TryGetInt32(out int intValue))
                    return intValue;
                if (reader.TryGetDouble(out double doubleValue))
                    return (int)Math.Round(doubleValue);
                break;
            case JsonTokenType.String:
                var str = reader.GetString();
                if (string.IsNullOrEmpty(str))
                    return null;
                var cleaned = str.Replace(",", "").Replace(".", "");
                if (int.TryParse(cleaned, out int result))
                    return result;
                return null;
            case JsonTokenType.Null:
                return null;
        }

        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteNumberValue(value.Value);
        else
            writer.WriteNullValue();
    }
}
