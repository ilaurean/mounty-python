using Commons.Contracts;

namespace Operative.Contracts;

public sealed record OrderAggregateDTO(
    Guid PK,
    string OrderID,
    string Name,
    string CustomerID,
    DateTime StartDate,
    OrderStates State,
    int BoardsCount,
    int ComponentsCount) : AggregateDTO(PK)
{ }
