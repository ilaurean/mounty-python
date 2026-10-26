using FluentValidation;

namespace Commons.Domain.Validators;

public class AuditableEntityValidator : Validator<AuditableEntity, AuditableEntityValidator>
{
    public static readonly DateTime minimumRegistrationDate = new(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    protected override void Configure()
    {
        Include(ValueObjectValidator.Instance);

        RuleFor(x => x.ID).NotEmpty().WithMessage($"{nameof(AuditableEntity.ID)} is required.")
            .MaximumLength(AuditableEntity.idLength).WithMessage($"{nameof(AuditableEntity.ID)} cannot exceed {AuditableEntity.idLength} characters.");

        RuleFor(x => x.CreateDate).GreaterThan(minimumRegistrationDate);

        When(x => x.UpdateDate.HasValue, () =>
        {
            RuleFor(x => x.UpdateDate).GreaterThan(p => p.CreateDate);
        });

        When(x => x.DeleteDate.HasValue, () =>
        {
            RuleFor(x => x.DeleteDate).GreaterThanOrEqualTo(p => p.UpdateDate);
        });
    }
}

public class ValueObjectValidator : Validator<ValueObject, ValueObjectValidator>
{
    protected override void Configure()
    {
        RuleFor(x => x.PK).Must(x => x != Guid.Empty).WithMessage($"{nameof(ValueObject.PK)} must be a valid GUID.");
    }
}
