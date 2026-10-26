using Commons.Services;
using Microsoft.EntityFrameworkCore;
using Operative.Services.Features.Operations;

namespace Operative.Services;

public class OperativeWarmupHostedService(IServiceCollection services, IServiceProvider provider) : WarmupHostedService(services, provider)
{
    public override async Task<bool> WarmupAsync(IServiceScope scope)
    {
        var isSuccessful = true;
        var movieKey = Guid.Empty;

        try
        {
            logger.Verbose("Warming up the {repo}...", nameof(OrderRepository));
            var repository = scope.ServiceProvider.GetRequiredService<OrderRepository>();
            var movie = await repository.Entities.AsNoTracking().FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            logger.Warning("Initializing {repo} failed: {ex}", nameof(OrderRepository), ex.ToString());
            isSuccessful = false;
        }

        return isSuccessful;
    }
}
