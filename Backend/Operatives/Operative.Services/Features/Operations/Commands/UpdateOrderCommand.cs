using Commons.Domain;
using Commons.Services;
using FluentValidation;
using Operative.Contracts;
using Operative.Services.Features.Operations.Domain;
using Operative.Services.Features.Operations.Domain.Validators;

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

        // TODO: use EF filtering/tenant possibility, skip the filtering when manager
        OrderOwnershipGuard.EnsureAccess(currentUser, order.CustomerID);

        order.UpdateNameOrDescription(request.Name, request.Description, request.OrderDate);
        if (request.Items is not null)
        {
            order.UpdateItems(MergeItems(order, request.Items)!);
        }

        BaseOrderValidator.Instance.ValidateAndThrow(order);
        await repository.SaveChangesAsync(cancellationToken);
        return order;
    }

    private List<OrderItem>? MergeItems(Order order, IEnumerable<CreateOrderItemRequest>? requests)
    {
        if (requests is null)
        {
            return null;
        }

        var position = 0;
        var result = new List<OrderItem>();
        foreach (var request in requests)
        {
            var existing = request.PK is null ? null : order.Items.FirstOrDefault(c => c.PK == request.PK);
            if (existing is null)
            {
                if (existing is null)
                {
                    var boards = OrderItemFactory.Build([request], position);
                    result.AddRange(boards);
                    position += boards.Count;
                    repository.Context.AddRange(boards);
                    continue;
                }
                else
                {
                    repository.Context.Attach(existing.Board);
                    existing.Update(++position, Board.Create
                        .WithID(request.BoardID)
                        .WithName(request.BoardName)
                        .WithComponents(request.Components.Select(comp => Component.Create
                            .WithName(comp.Name)
                            .WithQuantity(comp.Quantity)
                            .Build()).ToList())
                        .Build(), request.Quantity);
                }
                result.Add(existing);
            }
            result.Add(existing);
        }
        return result;
    }
}

internal static class OrderItemFactory
{
    public static IList<OrderItem> Build(IEnumerable<CreateOrderItemRequest> requests, int internalPosition) =>
        requests.Select(c => OrderItem.Create
            .WithPosition(++internalPosition)
            .WithBoard(Board.Create
                .WithID(c.BoardID)
                .WithName(c.BoardName)
                .WithComponents(c.Components.Select(comp => Component.Create
                    .WithName(comp.Name)
                    .WithQuantity(comp.Quantity)
                    .Build()).ToList())
                .Build())
            .WithQuantity(c.Quantity)
            .Build()).ToList();
}