using Commons.Domain;
using Commons.Domain.Validators;
using FluentValidation;

namespace Inventory.Services.Features.Catalogs.Domain.Validators;

public class BoardValidator : Validator<Board, BoardValidator>
{
    protected override void Configure()
    {
        Include(AuditableEntityValidator.Instance);

        RuleFor(x => x.ID).NotEmpty().WithMessage($"{nameof(Board.ID)} is required.")
            .MaximumLength(AuditableEntity.idLength).WithMessage($"{nameof(Board.ID)} cannot exceed {AuditableEntity.idLength} characters long.");
        RuleFor(x => x.Name).NotEmpty().WithMessage($"{nameof(Board.Name)} is required.")
            .MaximumLength(128).WithMessage($"{nameof(Board.Name)} cannot exceed 128 characters long.");
        RuleFor(x => x.Description).MaximumLength(1024).WithMessage($"{nameof(Board.Description)} cannot exceed 1024 characters long.");
        RuleFor(x => x.Length).GreaterThan(0).When(x => x.Length.HasValue).WithMessage($"{nameof(Board.Length)} must be greater than 0.");
        RuleFor(x => x.Width).GreaterThan(0).When(x => x.Width.HasValue).WithMessage($"{nameof(Board.Width)} must be greater than 0.");
        RuleFor(x => x.Components).NotNull().WithMessage($"{nameof(Board.Components)} is required.")
            .Must(x => x.Count > 0).WithMessage($"{nameof(Board)} must contain at least one component.")
            .ForEach(y => y.SetValidator(ComponentValidator.Instance));
    }
}
