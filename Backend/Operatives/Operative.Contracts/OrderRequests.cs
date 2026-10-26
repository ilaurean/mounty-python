namespace Operative.Contracts;

public sealed record CreateOrderComponentRequest(string Name, int Quantity);
public sealed record CreateOrderItemRequest(string BoardID, string BoardName, int Quantity, IList<CreateOrderComponentRequest> Components);
public sealed record CreateOrderRequest(string Name, string? Description, DateTime OrderDate, IList<CreateOrderItemRequest> Items);
public sealed record UpdateOrderRequest(string? Name, string? Description);
public sealed record OrderDownloadDTO(string FileName, string ContentType, byte[] Content);
