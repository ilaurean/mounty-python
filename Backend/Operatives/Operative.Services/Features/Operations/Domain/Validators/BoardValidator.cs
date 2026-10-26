using Commons.Domain;
using Commons.Domain.Validators;
using FluentValidation;

namespace Operative.Services.Features.Operations.Domain.Validators;

public class BoardValidator : Validator<Board, BoardValidator>
{
    protected override void Configure()
    {
        Include(ValueObjectValidator.Instance);
        RuleFor(x => x.ID).NotEmpty().WithMessage($"{nameof(Board.ID)} is required.")
            .MaximumLength(AuditableEntity.idLength).WithMessage($"{nameof(Board.ID)} cannot exceed {AuditableEntity.idLength} characters long.");
        RuleFor(x => x.Name).NotEmpty().WithMessage($"{nameof(Board.Name)} is required.")
            .MaximumLength(128).WithMessage($"{nameof(Board.Name)} cannot exceed 128 characters long.");
        RuleFor(x => x.Components).NotNull().WithMessage($"{nameof(Board.Components)} is required.")
            .ForEach(y => y.SetValidator(ComponentValidator.Instance));
    }
}
