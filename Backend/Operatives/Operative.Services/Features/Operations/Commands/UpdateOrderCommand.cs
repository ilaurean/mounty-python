using Commons.Domain;
using Commons.Services;
using Operative.Contracts;
using Operative.Services.Features.Operations.Domain;

namespace Operative.Services.Features.Operations.Commands;

public interface IUpdateOrderCommand
{
    Task<Order> ExecuteAsync(CurrentUser currentUser, string orderId, UpdateOrderRequest request, CancellationToken cancellationToken);
}

public class UpdateOrderCommand(IOrderRepository repository) : IUpdateOrderCommand
{
    public async Task<Order> ExecuteAsync(CurrentUser currentUser, string orderId, UpdateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIDWithDetailsAsync(orderId, cancellationToken)
            ?? throw new NotFoundException($"Order '{orderId}' was not found.");

        OrderOwnershipGuard.EnsureAccess(currentUser, order.CustomerID);

        order.UpdateNameOrDescription(request.Name, request.Description);
        await repository.SaveChangesAsync(cancellationToken);
        return order;
    }
}
