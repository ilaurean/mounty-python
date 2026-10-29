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
        if (request.Items is not null)
        {
            order.UpdateItems(request.Items.Select((item, index) =>
            {
                var components = item.Components.Select(c => new Component.ComponentBuilder()
                    .WithName(c.Name)
                    .WithQuantity(c.Quantity)
                    .Build()).ToList();

                var board = new Board.BoardBuilder()
                    .WithID(item.BoardID)
                    .WithName(item.BoardName)
                    .WithComponents(components)
                    .Build();

                return new OrderItem.OrderItemBuilder()
                    .WithPosition(index + 1)
                    .WithBoard(board)
                    .WithQuantity(item.Quantity)
                    .Build();
            }).ToList());
        }

        await repository.SaveChangesAsync(cancellationToken);
        return order;
    }
}
