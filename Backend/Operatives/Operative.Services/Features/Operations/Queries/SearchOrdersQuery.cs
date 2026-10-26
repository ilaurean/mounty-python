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
    public Task<IList<OrderAggregateDTO>> ExecuteAsync(CurrentUser currentUser, OrderFilterDTO filter, CancellationToken cancellationToken)
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
            query = query.Where(x => x.Name.Contains(filter.Name));
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

        var orders = query
            .OrderByDescending(x => x.OrderDate).ThenBy(x => x.Name)
            .Skip(filter.pageIndex ?? 0)
            .Take(filter.pageSize ?? 20)
            .Select(x => x.ToAggregateDTO())
            .ToList();
        return Task.FromResult<IList<OrderAggregateDTO>>(orders);
    }
}
