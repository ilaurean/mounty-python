using Commons.Domain;
using Commons.Services;
using Operative.Contracts;
using Operative.Services.Features.Operations.Domain;

namespace Operative.Services.Features.Operations.Queries;

public interface IGetOrderByIDQuery
{
    Task<OrderDTO> ExecuteAsync(CurrentUser currentUser, string orderId, CancellationToken cancellationToken);
}

public class GetOrderByIDQuery(IOrderRepository repository) : IGetOrderByIDQuery
{
    public async Task<OrderDTO> ExecuteAsync(CurrentUser currentUser, string orderId, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIDWithDetailsAsync(orderId, cancellationToken)
            ?? throw new NotFoundException($"Order '{orderId}' was not found.");

        if (!currentUser.isManager && order.CustomerID != currentUser.userId)
        {
            throw new NotFoundException($"Order '{orderId}' was not found.");
        }

        return order.ToDTO();
    }
}
