using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace Commons.Services;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Maps development endpoints (OpenAPI, Scalar).
    /// </summary>
    public static WebApplication MapDevelopmentEndpoints(this WebApplication app, string apiTitle)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.WithTitle(apiTitle);
            });
        }

        return app;
    }
}
