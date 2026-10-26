using Microsoft.EntityFrameworkCore;
using Operative.Contracts;
using Operative.Services.Features.Operations.Domain;

namespace Operative.Services.Infrastructure.Repositories;

public class OperativeSeedHostedService(IServiceProvider serviceProvider) : IHostedService
{
    // Must match the user ids defined in MountyPython.AppHost/import.json
    private const string testUser1 = "11111111-1111-4111-8111-111111111111";
    private const string testUser2 = "22222222-2222-4222-8222-222222222222";

    // Board ids/names/components match the dummy data of the inventory service
    private static readonly Dictionary<string, (string name, (string name, int quantity)[] components)> boards = new()
    {
        ["BRD-001"] = ("Starter Board", [("Microcontroller", 1), ("Resistor 10k", 20), ("LED Red", 8)]),
        ["BRD-002"] = ("Sensor Board", [("Temperature Sensor", 2), ("Humidity Sensor", 1), ("Capacitor 100nF", 15)]),
        ["BRD-003"] = ("Power Board", [("Voltage Regulator", 3), ("Inductor 10uH", 2), ("Fuse 2A", 2)]),
        ["BRD-004"] = ("Wireless Board", [("WiFi Module", 1), ("BLE Module", 1), ("Antenna", 2)]),
        ["BRD-005"] = ("Display Board", [("OLED Display", 1), ("Level Shifter", 2), ("Connector 8-pin", 4)]),
    };

    private static readonly (string name, string description, string customerId, int daysAgo, OrderStates status, (string boardId, int quantity)[] items)[] orders =
    [
        ("Prototype batch", "First prototype run", testUser1, 20, OrderStates.Completed, [("BRD-001", 5), ("BRD-002", 2)]),
        ("Sensor rollout", "Sensors for the lab", testUser1, 10, OrderStates.Processing, [("BRD-002", 10)]),
        ("Power upgrade", "Replacement power boards", testUser1, 2, OrderStates.Created, [("BRD-003", 4), ("BRD-001", 1)]),
        ("Wireless pilot", "Pilot of wireless boards", testUser2, 15, OrderStates.Cancelled, [("BRD-004", 3)]),
        ("Display demo", "Boards for trade show", testUser2, 5, OrderStates.Created, [("BRD-005", 6), ("BRD-004", 2)]),
    ];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OperativeDbContext>();

        if (await context.Set<Order>().AnyAsync(cancellationToken))
        {
            return;
        }

        foreach (var (name, description, customerId, daysAgo, status, items) in orders)
        {
            var order = Order.Create
                .WithName(name)
                .WithDescription(description)
                .WithCustomerID(customerId)
                .WithOrderDate(DateTime.UtcNow)
                .WithStatus(status)
                .WithItems([.. items.Select((item, index) => new OrderItem.OrderItemBuilder()
                    .WithPosition(index + 1)
                    .WithQuantity(item.quantity)
                    .WithBoard(BuildBoard(item.boardId))
                    .Build())])
                .Build();
            await context.Set<Order>().AddAsync(order, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static Board BuildBoard(string boardId)
    {
        var (name, components) = boards[boardId];
        return new Board.BoardBuilder()
            .WithID(boardId)
            .WithName(name)
            .WithComponents([.. components.Select(c => new Component.ComponentBuilder()
                .WithName(c.name)
                .WithQuantity(c.quantity)
                .Build())])
            .Build();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
