namespace Commons.Contracts;


public abstract record ValueObjectDTO(Guid PK);

public abstract record AuditableEntityDTO(
    Guid PK,
    string ID,
    DateTime? CreateDate,
    DateTime? UpdateDate,
    DateTime? DeleteDate) : ValueObjectDTO(PK);

public abstract record AggregateDTO(Guid PK) : ValueObjectDTO(PK);

public abstract record FilterDTO(int? pageIndex, int? pageSize);