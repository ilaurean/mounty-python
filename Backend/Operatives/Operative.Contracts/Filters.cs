using Commons.Contracts;

namespace Operative.Contracts;

public record OrderFilterDTO(
    string? OrderID,
    string? Name,
    string? CustomerID,
    OrderStates? State,
    string? BoardID) : FilterDTO
{ }