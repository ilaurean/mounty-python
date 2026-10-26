using Commons.Infrastructure.Messaging;
using Commons.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Operative.Services.Features.Operations.Domain;
using Operative.Services.Infrastructure.Repositories;

namespace Operative.Services.Features.Operations;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByIDWithDetailsAsync(string id, CancellationToken cancellationToken);
}

public class OrderRepository(OperativeDbContext context, IPublisher publisher) : Repository<Order, OperativeDbContext>(context, publisher), IOrderRepository
{
    public override IQueryable<Order> Entities => Context.Set<Order>()
        .Where(x => x.DeleteDate == null)
        .Include(o => o.Items)
            .ThenInclude(i => i.Board)
                .ThenInclude(b => b.Components);

    public override Task<Order?> GetByIDAsync(string id, CancellationToken cancellationToken)
    {
        return Entities.FirstOrDefaultAsync(o => o.ID == id, cancellationToken);
    }

    public Task<Order?> GetByIDWithDetailsAsync(string id, CancellationToken cancellationToken)
    {
        return GetByIDAsync(id, cancellationToken);
    }

    public override async Task<Order> AddAsync(Order entity, CancellationToken cancellationToken)
    {
        await Context.Set<Order>().AddAsync(entity, cancellationToken);
        return entity;
    }
}
