using Commons.Domain;
using FluentValidation;
using Operative.Services.Features.Operations.Domain.Validators;
using System.ComponentModel.DataAnnotations;

namespace Operative.Services.Features.Operations.Domain;

public sealed class OrderItem : ValueObject
{
    private OrderItem() { }

    public int Position { get; private set; }

    [Required]
    public Board Board { get; private set; }

    [Required]
    public int Quantity { get; private set; }

    public class OrderItemBuilder
    {
        private readonly OrderItem orderItem = new();
        public OrderItemBuilder WithPosition(int position) { orderItem.Position = position; return this; }
        public OrderItemBuilder WithBoard(Board board) { orderItem.Board = board; return this; }
        public OrderItemBuilder WithQuantity(int quantity) { orderItem.Quantity = quantity; return this; }
        public OrderItem Build()
        {
            OrderItemValidator.Instance.ValidateAndThrow(orderItem);
            return orderItem;
        }
    }
}