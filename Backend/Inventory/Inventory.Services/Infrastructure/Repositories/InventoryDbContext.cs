using Commons.Infrastructure.Repositories;
using Inventory.Services.Features.Catalogs.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Inventory.Services.Infrastructure.Repositories;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<Board> Orders => Set<Board>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseUnaccent();
    }
}

public class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=inventory;Username=<<postgres>>;Password=<<<yourpassword>>");

        return new InventoryDbContext(optionsBuilder.Options);
    }
}