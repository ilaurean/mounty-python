using Commons.Domain;
using FluentValidation;
using Inventory.Contracts.Events;
using Inventory.Services.Features.Catalogs.Domain.Validators;
using System.ComponentModel.DataAnnotations;

namespace Inventory.Services.Features.Catalogs.Domain;

public sealed class Board : AuditableEntity
{
    [Required]
    [StringLength(128)]
    public string Name { get; private set; }

    [StringLength(1024)]
    public string Description { get; private set; }

    public decimal? Length { get; private set; }

    public decimal? Width { get; private set; }

    public IList<Component> Components { get; private set; } = [];

    public static BoardBuilder Create => new();

    public void Update(string name, string? description, decimal? length, decimal? width, IList<Component>? components = null)
    {
        Name = name;
        Description = description!;
        Length = length;
        Width = width;
        if (components is not null)
        {
            Components = components;
        }

        BoardValidator.Instance.ValidateAndThrow(this);
        UpdateDate = DateTime.UtcNow;
        Events.Add(new BoardUpdatedEvent(this.ToDTO()));
    }

    public override void Delete(DateTime? datetime = null)
    {
        base.Delete(datetime);
        Events.Add(new BoardDeletedEvent(this.ToDTO()));
    }

    public class BoardBuilder
    {
        private readonly Board board = new() { PK = Guid.NewGuid() };
        public BoardBuilder WithID(string id) { board.ID = id; return this; }
        public BoardBuilder WithName(string name) { board.Name = name; return this; }
        public BoardBuilder WithDescription(string description) { board.Description = description; return this; }
        public BoardBuilder WithDimension(decimal? length, decimal? width) { board.Length = length; board.Width = width; return this; }
        public BoardBuilder WithComponents(IList<Component> components) { board.Components = components; return this; }

        public Board Build()
        {
            BoardValidator.Instance.ValidateAndThrow(board);
            board.Events.Add(new BoardCreatedEvent(board.ToDTO()));
            return board;
        }
    }
}
