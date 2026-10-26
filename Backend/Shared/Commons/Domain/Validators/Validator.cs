using FluentValidation;

namespace Commons.Domain.Validators;

public abstract class Validator<T, TValidator> : AbstractValidator<T> where TValidator : AbstractValidator<T>, new()
{
    protected Validator()
    {
        Configure();
    }

    public static TValidator Instance => lazy.Value;

    protected abstract void Configure();

    private static readonly Lazy<TValidator> lazy = new(() => Activator.CreateInstance<TValidator>());
}
