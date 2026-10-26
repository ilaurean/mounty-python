using Commons.Domain.Validators;
using FluentValidation;

namespace Operative.Services.Features.Operations.Domain.Validators;

public class OrderItemValidator : Validator<OrderItem, OrderItemValidator>
{
    protected override void Configure()
    {
        Include(ValueObjectValidator.Instance);

        RuleFor(x => x.Position).GreaterThan(0).WithMessage($"{nameof(OrderItem.Position)} must be greater than 0.");
        RuleFor(x => x.Board).NotNull().WithMessage($"{nameof(OrderItem.Board)} is required.")
            .SetValidator(BoardValidator.Instance);
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage($"{nameof(OrderItem.Quantity)} must be greater than 0.");
    }
}
