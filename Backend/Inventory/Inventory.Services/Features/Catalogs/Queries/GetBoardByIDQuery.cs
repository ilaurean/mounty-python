using Commons.Domain;
using Commons.Services;
using Inventory.Contracts;
using Inventory.Services.Features.Catalogs.Domain;

namespace Inventory.Services.Features.Catalogs.Queries;

public interface IGetBoardByIDQuery
{
    Task<BoardDTO> ExecuteAsync(CurrentUser currentUser, string boardId, CancellationToken cancellationToken);
}

public class GetBoardByIDQuery(ICatalogRepository repository) : IGetBoardByIDQuery
{
    public async Task<BoardDTO> ExecuteAsync(CurrentUser currentUser, string boardId, CancellationToken cancellationToken)
    {
        var board = await repository.GetByIDAsync(boardId, cancellationToken);
        if (board is null || board.IsDisabled)
        {
            throw new NotFoundException($"Board '{boardId}' was not found.");
        }

        return board.ToDTO();
    }
}
