using Commons.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Operative.Contracts;
using Operative.Contracts.Events;
using Operative.Services.Features.Operations.Domain.Validators;
using System.ComponentModel.DataAnnotations;

namespace Operative.Services.Features.Operations.Domain;

[Index(nameof(ID), IsUnique = true)]
[Index(nameof(Name), nameof(Description))]
public sealed class Order : AuditableEntity
{
    private Order() { }

    [Required]
    [StringLength(128)]
    public string Name { get; private set; }

    [StringLength(1024)]
    public string? Description { get; private set; }

    [Required]
    [StringLength(256)]
    public string CustomerID { get; private set; }

    public IList<OrderItem> Items { get; private set; } = [];

    public DateTime OrderDate { get; private set; }

    public OrderStates Status { get; private set; } = OrderStates.Created;

    public static OrderBuilder Create = new();

    public void UpdateNameOrDescription(string? name, string? description)
    {
        Name = name ?? Name;
        Description = description;
        BaseOrderValidator.Instance.ValidateAndThrow(this);
        Events.Add(new OrderUpdatedNameOrDescriptionEvent(this.ToDTO()));
    }

    public void StartProcessing()
    {
        IsOrderProcesableValidator.Instance.ValidateAndThrow(this);
        Status = OrderStates.Processing;
        Events.Add(new OrderStartProcessingEvent(this.ToDTO()));
    }

    public void Complete()
    {
        IsOrderCompletableValidator.Instance.ValidateAndThrow(this);
        Status = OrderStates.Completed;
        Events.Add(new OrderCompletedEvent(this.ToDTO()));
    }

    public void Cancel()
    {
        IsOrderCancellableValidator.Instance.ValidateAndThrow(this);
        Status = OrderStates.Cancelled;
        Events.Add(new OrderCancelledEvent(this.ToDTO()));
    }

    public class OrderBuilder
    {
        private readonly Order order = new()
        {
            PK = Guid.NewGuid(),
            ID = GenerateUniqueID(),
            CreateDate = DateTime.UtcNow,
        };

        public OrderBuilder WithName(string name) { order.Name = name; return this; }
        public OrderBuilder WithDescription(string description) { order.Description = description; return this; }
        public OrderBuilder WithCustomerID(string customerID) { order.CustomerID = customerID; return this; }
        public OrderBuilder WithItems(IList<OrderItem> items) { order.Items = items; return this; }
        public OrderBuilder WithCreateDate(DateTime createDate) { order.CreateDate = createDate; return this; }
        public OrderBuilder WithStatus(OrderStates status) { order.Status = status; return this; }

        public Order Build()
        {
            BaseOrderValidator.Instance.ValidateAndThrow(order);
            return order;
        }
    }
}
