using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Commons.Services;

public class DateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var stringValue = reader.GetString();

        if (string.IsNullOrEmpty(stringValue))
        {
            return default;
        }

        if (DateTime.TryParse(stringValue, null, System.Globalization.DateTimeStyles.RoundtripKind, out var result))
        {
            return result;
        }

        // Log warning similar to the obsolete version
        Debug.WriteLine($"Deserializing DateTime '{stringValue}' failed");
        return default;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("O"));
    }
}
