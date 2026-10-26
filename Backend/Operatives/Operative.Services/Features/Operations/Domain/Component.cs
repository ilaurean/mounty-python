using Commons.Domain;
using FluentValidation;
using Operative.Services.Features.Operations.Domain.Validators;
using System.ComponentModel.DataAnnotations;

namespace Operative.Services.Features.Operations.Domain;

public sealed class Component : ValueObject
{
    private Component() { }

    [Required]
    [StringLength(128)]
    public string Name { get; private set; }

    public int Quantity { get; private set; }

    public sealed class ComponentBuilder
    {
        private readonly Component component = new();

        public ComponentBuilder WithName(string name) { component.Name = name; return this; }
        public ComponentBuilder WithQuantity(int quantity) { component.Quantity = quantity; return this; }

        public Component Build()
        {
            ComponentValidator.Instance.ValidateAndThrow(component);
            return component;
        }
    }
}
