using Commons.Domain;
using Commons.Services;
using Operative.Services.Features.Operations.Domain;

namespace Operative.Services.Features.Operations.Commands;

public interface ICompleteOrderCommand
{
    Task<Order> ExecuteAsync(CurrentUser currentUser, string orderId, CancellationToken cancellationToken);
}

public class CompleteOrderCommand(IOrderRepository repository) : ICompleteOrderCommand
{
    public async Task<Order> ExecuteAsync(CurrentUser currentUser, string orderId, CancellationToken cancellationToken)
    {
        currentUser.EnsureManager();

        var order = await repository.GetByIDWithDetailsAsync(orderId, cancellationToken)
            ?? throw new NotFoundException($"Order '{orderId}' was not found.");

        order.Complete();
        await repository.SaveChangesAsync(cancellationToken);
        return order;
    }
}
