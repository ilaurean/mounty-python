using Commons.Contracts;
using Inventory.Contracts;

namespace Operative.Contracts;

public enum OrderStates : byte
{
    Created = 0,
    Processing = 1,
    Completed = 2,
    Cancelled = 3
}

public record OrderDTO(
    Guid PK,
    string ID,
    DateTime? CreateDate,
    DateTime? UpdateDate,
    DateTime? DeleteDate,
    string CustomerID,
    string Name,
    string? Description,
    IList<OrderItemDTO> Items,
    DateTime OrderDate,
    OrderStates Status) : AuditableEntityDTO(PK, ID, CreateDate, UpdateDate, DeleteDate)
{ }

public record OrderItemDTO(Guid PK, int Position, BoardDTO Board, int Quantity) : ValueObjectDTO(PK) { }