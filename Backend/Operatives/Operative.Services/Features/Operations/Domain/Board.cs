using Commons.Domain;
using FluentValidation;
using Operative.Services.Features.Operations.Domain.Validators;
using System.ComponentModel.DataAnnotations;

namespace Operative.Services.Features.Operations.Domain;

public sealed class Board : ValueObject
{
    [Required]
    [StringLength(10)]
    public string ID { get; private set; }

    [Required]
    [StringLength(128)]
    public string Name { get; private set; }

    public IList<Component> Components { get; private set; } = [];

    public class BoardBuilder
    {
        private readonly Board board = new() { PK = Guid.NewGuid() };
        public BoardBuilder WithID(string id) { board.ID = id; return this; }
        public BoardBuilder WithName(string name) { board.Name = name; return this; }
        public BoardBuilder WithComponents(IList<Component> components) { board.Components = components; return this; }
        public Board Build()
        {
            BoardValidator.Instance.ValidateAndThrow(board);
            return board;
        }
    }
}
