using Commons.Domain.Validators;
using FluentValidation;
using Operative.Contracts;

namespace Operative.Services.Features.Operations.Domain.Validators;

public class BaseOrderValidator : Validator<Order, BaseOrderValidator>
{
    protected override void Configure()
    {
        Include(AuditableEntityValidator.Instance);

        RuleFor(x => x.Name).NotEmpty().WithMessage($"{nameof(Order.Name)} is required.")
            .MaximumLength(128).WithMessage($"{nameof(Order.Name)} cannot exceed 128 characters long.");
        RuleFor(x => x.Description).MaximumLength(1024).WithMessage($"{nameof(Order.Description)} cannot exceed 1024 characters long.");
        RuleFor(x => x.CustomerID).NotEmpty().WithMessage($"{nameof(Order.CustomerID)} is required.")
            .MaximumLength(256).WithMessage($"{nameof(Order.CustomerID)} cannot exceed 256 characters long.");
        RuleFor(x => x.Items).NotEmpty().WithMessage($"{nameof(Order.Items)} is required.")
            .Must(x => x.Count > 0).WithMessage($"{nameof(Order)} must contain at least one board/item.")
            .ForEach(y => y.SetValidator(OrderItemValidator.Instance));
        RuleFor(x => x.OrderDate.Date).GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage($"{nameof(Order.OrderDate)} cannot be in the past days.");
    }
}

public class IsOrderProcesableValidator : Validator<Order, IsOrderProcesableValidator>
{
    protected override void Configure()
    {
        RuleFor(x => x.Status).Equal(OrderStates.Created).WithMessage($"Only orders with status {OrderStates.Created} can be processed.");
    }
}

public class IsOrderCancellableValidator : Validator<Order, IsOrderCancellableValidator>
{
    protected override void Configure()
    {
        RuleFor(x => x.Status).Equal(OrderStates.Created).WithMessage($"Only orders with status {OrderStates.Created} can be cancelled.");
    }
}

public class IsOrderCompletableValidator : Validator<Order, IsOrderCompletableValidator>
{
    protected override void Configure()
    {
        RuleFor(x => x.Status).Equal(OrderStates.Processing).WithMessage($"Only orders with status {OrderStates.Processing} can be completed.");
    }
}
