using Commons.Infrastructure.Messaging;
using Commons.Infrastructure.Repositories;
using Inventory.Services.Features.Catalogs.Domain;
using Inventory.Services.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Services.Features.Catalogs;

public interface ICatalogRepository : IRepository<Board>
{
}

public class CatalogRepository(InventoryDbContext context, IPublisher publisher) : Repository<Board, InventoryDbContext>(context, publisher), ICatalogRepository
{
    public override IQueryable<Board> Entities => Context.Set<Board>()
        .Where(x => x.DeleteDate == null)
        .Include(b => b.Components);

    public override Task<Board?> GetByIDAsync(string id, CancellationToken cancellationToken)
    {
        return Entities.FirstOrDefaultAsync(b => b.ID == id, cancellationToken);
    }

    public override async Task<Board> AddAsync(Board entity, CancellationToken cancellationToken)
    {
        await Context.Set<Board>().AddAsync(entity, cancellationToken);
        return entity;
    }
}
