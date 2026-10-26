using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Hosting;

namespace Commons.Infrastructure.Repositories;

public static class NpgsqlExtensions
{
    public static IHostApplicationBuilder SetupNpgsqlServerDbContext<TContext>(this WebApplicationBuilder builder, string connectionName) where TContext : DbContext
    {
        builder.AddNpgsqlDbContext<TContext>(connectionName,
            settings =>
            {
                settings.CommandTimeout = 120;
            },
            options =>
            {
                options.ConfigureWarnings(w => w.Log(RelationalEventId.PendingModelChangesWarning));
                options.ConfigureWarnings(w => w.Ignore(RelationalEventId.MultipleCollectionIncludeWarning));
                options.ConfigureWarnings(w => w.Ignore(CoreEventId.InvalidIncludePathError));
                if (builder.Environment.IsDevelopment())
                {
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                }
            });
        return builder;
    }
}
