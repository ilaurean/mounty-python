using Commons.Contracts;

namespace Operative.Contracts;

public record OrderFilterDTO(
    string? OrderID,
    string? Name,
    string? CustomerID,
    OrderStates? State,
    string? BoardID,
    int? pageIndex = 0,
    int? pageSize = 20
    ) : FilterDTO(pageIndex, pageSize)
{ }