using Commons.Domain;
using Commons.Services;
using Operative.Services.Features.Operations.Domain;

namespace Operative.Services.Features.Operations.Commands;

public interface ICancelOrderCommand
{
    Task<Order> ExecuteAsync(CurrentUser currentUser, string orderId, CancellationToken cancellationToken);
}

public class CancelOrderCommand(IOrderRepository repository) : ICancelOrderCommand
{
    public async Task<Order> ExecuteAsync(CurrentUser currentUser, string orderId, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIDWithDetailsAsync(orderId, cancellationToken)
            ?? throw new NotFoundException($"Order '{orderId}' was not found.");

        OrderOwnershipGuard.EnsureAccess(currentUser, order.CustomerID);

        order.Cancel();
        await repository.SaveChangesAsync(cancellationToken);
        return order;
    }
}
