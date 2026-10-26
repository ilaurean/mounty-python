using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Commons.Infrastructure.Repositories;

public class MigratorHostedService<T>(IServiceProvider serviceProvider) : IHostedService where T : DbContext
{
    private readonly ILogger? logger = serviceProvider.GetService<ILogger>();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger?.Information($"Migrate database for {typeof(T).Name} if necessary");
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<T>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
