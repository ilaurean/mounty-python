using Commons.Services;
using Microsoft.EntityFrameworkCore;
using Operative.Contracts;
using Operative.Services.Features.Operations.Domain;

namespace Operative.Services.Features.Operations.Queries;

public interface ISearchOrdersQuery
{
    Task<IList<OrderAggregateDTO>> ExecuteAsync(CurrentUser currentUser, OrderFilterDTO filter, CancellationToken cancellationToken);
}

public class SearchOrdersQuery(IOrderRepository repository) : ISearchOrdersQuery
{
    public async Task<IList<OrderAggregateDTO>> ExecuteAsync(CurrentUser currentUser, OrderFilterDTO filter, CancellationToken cancellationToken)
    {
        var query = repository.Entities.AsNoTracking();

        query = currentUser.isManager
            ? query
            : query.Where(x => x.CustomerID == currentUser.userId);

        if (!string.IsNullOrWhiteSpace(filter.OrderID))
        {
            query = query.Where(x => x.ID == filter.OrderID);
        }

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{filter.Name}%"));
        }

        if (currentUser.isManager && !string.IsNullOrWhiteSpace(filter.CustomerID))
        {
            query = query.Where(x => x.CustomerID == filter.CustomerID);
        }

        if (filter.State.HasValue)
        {
            query = query.Where(x => x.Status == filter.State.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.BoardID))
        {
            query = query.Where(x => x.Items.Any(y => y.Board.ID == filter.BoardID));
        }

        var orders = await query
             .Include(x => x.Items)
                .ThenInclude(y => y.Board)
                .ThenInclude(z => z.Components)
            .OrderByDescending(x => x.OrderDate).ThenBy(x => x.Name)
            .Skip(filter.pageIndex ?? 0)
            .Take(filter.pageSize ?? 20)
            .Select(x => x.ToAggregateDTO())
            .ToListAsync(cancellationToken);
        return await Task.FromResult<IList<OrderAggregateDTO>>(orders);
    }
}
