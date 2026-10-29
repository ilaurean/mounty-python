using Inventory.Contracts;
using Operative.Contracts;

namespace Operative.Services.Features.Operations.Domain;

public static class DomainExtensions
{
    public static OrderDTO ToDTO(this Order order)
    {
        return new OrderDTO
        (
            order.PK,
            order.ID,
            order.CreateDate,
            order.UpdateDate,
            order.DeleteDate,
            order.CustomerID,
            order.Name,
            order.Description,
            order.Items.Select(i => i.ToDTO()).ToList(),
            order.OrderDate,
            order.Status
        );
    }

    /// <summary>Lightweight aggregate projection used by order search/list results (see copilot-instructions.md: queries return aggregate DTOs).</summary>
    public static OrderAggregateDTO ToAggregateDTO(this Order order)
    {
        return new OrderAggregateDTO
        (
            order.PK,
            order.ID,
            order.Name,
            order.CustomerID,
            order.OrderDate,
            order.Status,
            order.Items.Count,
            order.Items.Sum(i => i.Board.Components.Count)
        );
    }

    public static OrderItemDTO ToDTO(this OrderItem item)
    {
        return new OrderItemDTO
        (
            item.PK,
            item.Position,
            item.Board.ToDTO(),
            item.Quantity
        );
    }

    public static BoardDTO ToDTO(this Board board)
    {
        return new BoardDTO
        (
            board.PK,
            board.ID,
            null,
            null,
            null,
            board.Name,
            null,
            null,
            null,
            board.Components.Select(c => c.ToDTO()).ToList()
        );
    }

    public static ComponentDTO ToDTO(this Component component)
    {
        return new ComponentDTO
        (
            component.PK,
            component.Name,
            null,
            component.Quantity
        );
    }
}