using Commons.Services;
using Operative.Contracts;
using Operative.Services.Features.Operations.Domain;

namespace Operative.Services.Features.Operations.Commands;

public interface ICreateOrderCommand
{
    Task<Order> ExecuteAsync(CurrentUser currentUser, CreateOrderRequest request, CancellationToken cancellationToken);
}

public class CreateOrderCommand(IOrderRepository repository) : ICreateOrderCommand
{
    public async Task<Order> ExecuteAsync(CurrentUser currentUser, CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var items = request.Items.Select((item, index) =>
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
        }).ToList();

        var order = Order.Create
            .WithName(request.Name)
            .WithDescription(request.Description!)
            .WithCustomerID(currentUser.userId)
            .WithItems(items)
            .WithOrderDate(request.OrderDate.ToUniversalTime())
            .Build();

        await repository.AddAsync(order, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return order;
    }
}
