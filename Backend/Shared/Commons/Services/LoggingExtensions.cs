using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Enrichers.Sensitive;

namespace Commons.Services;

public static class LoggingExtensions
{
    public static void SetupLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = LoggingExtensions.CreateLogger(builder.Configuration);
        builder.Host.UseSerilog(Log.Logger);
        builder.Services.AddSingleton<ILogger>(Log.Logger);
    }

    public static readonly IMaskingOperator[] MaskingOperators =
     [
        new MacAddressOperator(),
        new EmailAddressMaskingOperator()
     ];

    public static ILogger CreateLogger(IConfiguration configuration)
    {
        return new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithCorrelationId()
            .Enrich.WithSensitiveDataMasking(options => options.MaskingOperators = [.. MaskingOperators])
            .WriteTo.Console(outputTemplate: "[{Timestamp:o} {CorrelationId} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
}

/// <summary>
/// Based on datadog Standard MAC Address Scanner
/// Matches a sequence of characters representing a MAC address.
/// Examples of matching formats:
/// - FF:FF:FF:FF:FF:FF
/// - FF-FF-FF-FF-FF-FF
/// </summary>
public class MacAddressOperator : RegexMaskingOperator
{
    private const string regexString = @"[\b\s+\t][0-9a-fA-F]{2}[:-][0-9a-fA-F]{2}[:-][0-9a-fA-F]{2}[:-][0-9a-fA-F]{2}[:-][0-9a-fA-F]{2}[:-][0-9a-fA-F]{2}[b\s+\t.]";

    public MacAddressOperator() : base(regexString)
    {
    }
}
