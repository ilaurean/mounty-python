using Commons.Services;
using Inventory.Contracts;
using Inventory.Services.Features.Catalogs.Domain;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Services.Features.Catalogs.Queries;

public interface ISearchBoardsQuery
{
    Task<IList<BoardDTO>> ExecuteAsync(CurrentUser currentUser, BoardFilterDTO filter, CancellationToken cancellationToken);
}

public class SearchBoardsQuery(ICatalogRepository repository) : ISearchBoardsQuery
{
    public Task<IList<BoardDTO>> ExecuteAsync(CurrentUser currentUser, BoardFilterDTO filter, CancellationToken cancellationToken)
    {
        var query = repository.Entities.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.BoardID))
        {
            query = query.Where(b => b.ID == filter.BoardID);
        }

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            query = query.Where(b => b.Name.Contains(filter.Name));
        }

        IList<BoardDTO> result = query.ToList().Select(b => b.ToDTO()).ToList();
        return Task.FromResult(result);
    }
}
