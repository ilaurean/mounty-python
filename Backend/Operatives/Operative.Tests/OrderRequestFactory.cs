using Operative.Contracts;

namespace Operative.Tests;

internal static class OrderRequestFactory
{
    public static CreateOrderRequest ValidRequest() => new(
        "Order 1", "Description", DateTime.UtcNow,
        [new CreateOrderItemRequest("BRD-001", "Board 1", 2, [new CreateOrderComponentRequest("Resistor", 10)])]);
}
