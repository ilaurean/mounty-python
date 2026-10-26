using Commons.Contracts;

namespace Inventory.Contracts;

public sealed record ComponentRequest(string Name, string? Description, int Quantity);
public sealed record CreateBoardRequest(string ID, string Name, string? Description, decimal? Length, decimal? Width, IList<ComponentRequest> Components);
public sealed record UpdateBoardRequest(string Name, string? Description, decimal? Length, decimal? Width, IList<ComponentRequest>? Components);
public sealed record BoardFilterDTO(string? BoardID, string? Name, int? pageIndex = 0, int? pageSize = 20) : FilterDTO(pageIndex, pageSize);
