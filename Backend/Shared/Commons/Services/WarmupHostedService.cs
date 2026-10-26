using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ILogger = Serilog.ILogger;

namespace Commons.Services;

public abstract class WarmupHostedService(IServiceCollection services, IServiceProvider provider) : IHostedService
{
    protected readonly ILogger logger = (ILogger)provider.GetService(typeof(ILogger))!;

    public abstract Task<bool> WarmupAsync(IServiceScope scope);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (provider == null)
        {
            logger.Warning("Argument {Provider} is not initialized in {ServiceWarmupTask}", provider, nameof(WarmupHostedService));
            return;
        }

        var isSuccessful = true;
        using (var scope = provider.CreateScope())
        {
            logger.Verbose("Warming up the static constructs and instances...");
            foreach (var service in GetConfiguredService(services))
            {
                try
                {
                    scope.ServiceProvider.GetServices(service);
                }
                catch (Exception)
                {
                }
            }

            logger.Information("Warming up the dedicated services...");

            isSuccessful = await WarmupAsync(scope);
            if (isSuccessful)
            {
                logger.Information("...warming up done! No errors found!");
            }
            else
            {
                logger.Information("...warming up done! But with errors!");
            }
        }
        await Task.FromResult(isSuccessful);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    private static IEnumerable<Type> GetConfiguredService(IServiceCollection services)
    {
        return services
            .Where(x => x.ImplementationType != typeof(WarmupHostedService))
            .Where(x => x.ServiceType.ContainsGenericParameters == false)
            .Select(x => x.ServiceType)
            .Distinct();
    }

}
