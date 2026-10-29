using Commons.Services;
using Inventory.Services.Features.Catalogs;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Services;

public class InventoryWarmupHostedService(IServiceCollection services, IServiceProvider provider) : WarmupHostedService(services, provider)
{
    public override async Task<bool> WarmupAsync(IServiceScope scope)
    {
        var isSuccessful = true;
        var movieKey = Guid.Empty;

        try
        {
            logger.Verbose("Warming up the {repo}...", nameof(CatalogRepository));
            var repository = scope.ServiceProvider.GetRequiredService<CatalogRepository>();
            var movie = await repository.Entities.AsNoTracking().FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            logger.Warning("Initializing {repo} failed: {ex}", nameof(CatalogRepository), ex.ToString());
            isSuccessful = false;
        }

        return isSuccessful;
    }
}
