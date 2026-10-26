using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Commons.Services;

public static class JsonSerializerDefaults
{
    // Read-only to prevent accidental modification  
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true, // Optional: Use false in production for minified JSON
        //ReferenceHandler = ReferenceHandler.Preserve,
        Converters =
        {
            new JsonStringEnumConverter(), // Serialize enums as strings
            new DateTimeConverter(),
            new IntConverterFactory(),
            new BoolConverterFactory()
        }
    };

    public static IServiceCollection SetupJsonDefaults(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonSerializerDefaults.Options.PropertyNamingPolicy;
            options.SerializerOptions.PropertyNameCaseInsensitive = JsonSerializerDefaults.Options.PropertyNameCaseInsensitive;
            options.SerializerOptions.DefaultIgnoreCondition = JsonSerializerDefaults.Options.DefaultIgnoreCondition;
            options.SerializerOptions.WriteIndented = JsonSerializerDefaults.Options.WriteIndented;
            //options.SerializerOptions.ReferenceHandler = JsonSerializerDefaults.Options.ReferenceHandler;
            options.SerializerOptions.Converters.Clear();
            foreach (var converter in JsonSerializerDefaults.Options.Converters)
            {
                options.SerializerOptions.Converters.Add(converter);
            }
        });
        return services;
    }
}