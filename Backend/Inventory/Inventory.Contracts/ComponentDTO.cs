using Commons.Contracts;

namespace Inventory.Contracts;

public record ComponentDTO(
    Guid PK,
    string Name,
    string? Description,
    int Quantity) : ValueObjectDTO(PK)
{ }
