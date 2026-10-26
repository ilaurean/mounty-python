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
            : query.Where(o => o.CustomerID == currentUser.userId);

        if (!string.IsNullOrWhiteSpace(filter.OrderID))
        {
            query = query.Where(o => o.ID == filter.OrderID);
        }

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            query = query.Where(o => o.Name.Contains(filter.Name));
        }

        if (currentUser.isManager && !string.IsNullOrWhiteSpace(filter.CustomerID))
        {
            query = query.Where(o => o.CustomerID == filter.CustomerID);
        }

        if (filter.State.HasValue)
        {
            query = query.Where(o => o.Status == filter.State.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.BoardID))
        {
            query = query.Where(o => o.Items.Any(i => i.Board.ID == filter.BoardID));
        }

        var orders = query.ToList();
        return Task.FromResult<IList<OrderAggregateDTO>>(orders.Select(o => o.ToAggregateDTO()).ToList());
    }
}
