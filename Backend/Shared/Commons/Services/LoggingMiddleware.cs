using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Events;
using System.Diagnostics;

namespace Commons.Services;

public class LoggingMiddleware(RequestDelegate next, ILogger logger)
{
    private readonly RequestDelegate next = next ?? throw new ArgumentNullException(nameof(next));

    public virtual async Task Invoke(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var stopwatch = Stopwatch.StartNew();
        // enable buffering of the request to allow reading the body after the controller has consumed it
        context.Request.EnableBuffering();

        var originalBody = context.Response.Body;
        using var newBody = new MemoryStream();
        context.Response.Body = newBody;

        try
        {
            await next(context);
            stopwatch.Stop();

            var statusCode = context.Response?.StatusCode;
            var level = statusCode.GetValueOrDefault(0) > 499 ? LogEventLevel.Error : LogEventLevel.Information;

            // Skip OPTIONS
            if (HttpMethods.IsOptions(context.Request.Method))
            {
                return;
            }

            logger.Write(level, "Request: [{method}] {scheme}:~{path}{query}, returns status: {status} in {timestamp}", context.Request.Method?.ToUpperInvariant(), context.Request.Scheme, context.Request.Path, context.Request.QueryString.ToString(), statusCode, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.Error("Request failed in {timestamp} with the message: {message}", stopwatch.Elapsed, ex);
            throw;
        }
        finally
        {
            newBody.Seek(0, SeekOrigin.Begin);
            var bodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            logger.Verbose("Response Body: {bodyText}", bodyText);
            newBody.Seek(0, SeekOrigin.Begin);
            await newBody.CopyToAsync(originalBody);
        }
    }
}
