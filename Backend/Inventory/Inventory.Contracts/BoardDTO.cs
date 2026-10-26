using Commons.Contracts;

namespace Inventory.Contracts;

public record BoardDTO(
    Guid PK,
    string ID,
    DateTime? CreateDate,
    DateTime? UpdateDate,
    DateTime? DeleteDate,
    string Name,
    string? Description,
    decimal? Length,
    decimal? Width,
    IList<ComponentDTO> Components) : AuditableEntityDTO(PK, ID, CreateDate, UpdateDate, DeleteDate)
{ }
