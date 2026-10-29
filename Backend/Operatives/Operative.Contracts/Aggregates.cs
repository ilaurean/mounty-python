using Commons.Contracts;

namespace Operative.Contracts;

public sealed record OrderAggregateDTO(
    Guid PK,
    string ID,
    string Name,
    string CustomerID,
    DateTime OrderDate,
    OrderStates Status,
    int BoardsCount,
    int ComponentsCount) : AggregateDTO(PK)
{ }
