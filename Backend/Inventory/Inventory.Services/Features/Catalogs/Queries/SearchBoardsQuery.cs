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
    public async Task<IList<BoardDTO>> ExecuteAsync(CurrentUser currentUser, BoardFilterDTO filter, CancellationToken cancellationToken)
    {
        var query = repository.Entities.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.BoardID))
        {
            query = query.Where(x => x.ID == filter.BoardID);
        }

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{filter.Name}%"));
            //query = query.Where(x => x.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase));
        }

        var result = await query
            .OrderBy(x => x.Name)
            .Skip(filter.pageIndex ?? 0)
            .Take(filter.pageSize ?? 20)
            .Select(x => x.ToDTO()).ToListAsync(cancellationToken);
        return await Task.FromResult(result);
    }
}
