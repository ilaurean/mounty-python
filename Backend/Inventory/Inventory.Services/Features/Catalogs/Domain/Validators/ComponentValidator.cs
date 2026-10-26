using Commons.Domain.Validators;
using FluentValidation;

namespace Inventory.Services.Features.Catalogs.Domain.Validators;

public class ComponentValidator : Validator<Component, ComponentValidator>
{
    protected override void Configure()
    {
        Include(ValueObjectValidator.Instance);

        RuleFor(x => x.Name).NotEmpty().WithMessage($"{nameof(Component.Name)} is required.")
            .MaximumLength(128).WithMessage($"{nameof(Component.Name)} cannot exceed 128 characters long.");
        RuleFor(x => x.Description).MaximumLength(1024).WithMessage($"{nameof(Component.Description)} cannot exceed 1024 characters long.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage($"{nameof(Component.Quantity)} must be greater than 0.");
    }
}
