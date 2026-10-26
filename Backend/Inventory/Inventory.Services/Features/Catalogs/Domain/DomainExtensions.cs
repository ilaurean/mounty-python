using Inventory.Contracts;

namespace Inventory.Services.Features.Catalogs.Domain;

public static class DomainExtensions
{
    public static BoardDTO ToDTO(this Board board)
    {
        return new BoardDTO
        (
            board.PK,
            board.ID,
            board.CreateDate,
            board.UpdateDate,
            board.DeleteDate,
            board.Name,
            board.Description,
            board.Length,
            board.Width,
            board.Components.Select(c => c.ToDTO()).ToList()
        );
    }

    public static ComponentDTO ToDTO(this Component component)
    {
        return new ComponentDTO
        (
            component.PK,
            component.Name,
            component.Description,
            component.Quantity
        );
    }
}