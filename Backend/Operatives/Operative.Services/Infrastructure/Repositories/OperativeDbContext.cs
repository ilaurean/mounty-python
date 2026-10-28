using Commons.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Operative.Services.Features.Operations.Domain;

namespace Operative.Services.Infrastructure.Repositories;

public class OperativeDbContext(DbContextOptions<OperativeDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseUnaccent();
    }
}

public class OperativeDbContextFactory : IDesignTimeDbContextFactory<OperativeDbContext>
{
    public OperativeDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OperativeDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=operative;Username=<<postgres>>;Password=<<<yourpassword>>");

        return new OperativeDbContext(optionsBuilder.Options);
    }
}