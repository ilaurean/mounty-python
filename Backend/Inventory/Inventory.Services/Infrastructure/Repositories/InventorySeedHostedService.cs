using Inventory.Services.Features.Catalogs.Domain;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Services.Infrastructure.Repositories;

public class InventorySeedHostedService(IServiceProvider serviceProvider) : IHostedService
{
    public static readonly (string id, string name, string description, decimal length, decimal width, (string name, string description, int quantity)[] components)[] Boards =
    [
        ("BRD-001", "Starter Board", "Entry-level development board", 80, 50,
            [("Microcontroller", "ARM Cortex-M4 MCU", 1), ("Resistor 10k", "Through-hole resistor", 20), ("LED Red", "5mm red LED", 8)]),
        ("BRD-002", "Sensor Board", "Board with environmental sensors", 100, 60,
            [("Temperature Sensor", "Digital temperature sensor", 2), ("Humidity Sensor", "Capacitive humidity sensor", 1), ("Capacitor 100nF", "Ceramic capacitor", 15)]),
        ("BRD-003", "Power Board", "Voltage regulation and distribution", 120, 80,
            [("Voltage Regulator", "3.3V LDO regulator", 3), ("Inductor 10uH", "Power inductor", 2), ("Fuse 2A", "Resettable fuse", 2)]),
        ("BRD-004", "Wireless Board", "BLE and WiFi connectivity module", 90, 55,
            [("WiFi Module", "2.4GHz WiFi module", 1), ("BLE Module", "Bluetooth Low Energy module", 1), ("Antenna", "PCB trace antenna", 2)]),
        ("BRD-005", "Display Board", "OLED display driver board", 70, 45,
            [("OLED Display", "0.96 inch OLED", 1), ("Level Shifter", "Bidirectional level shifter", 2), ("Connector 8-pin", "Header connector", 4)]),
    ];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        if (await context.Set<Board>().AnyAsync(cancellationToken))
        {
            return;
        }

        foreach (var (id, name, description, length, width, components) in Boards)
        {
            var board = Board.Create
                .WithID(id)
                .WithName(name)
                .WithDescription(description)
                .WithDimension(length, width)
                .WithComponents([.. components.Select(c => new Component.ComponentBuilder()
                    .WithName(c.name)
                    .WithDescription(c.description)
                    .WithQuantity(c.quantity)
                    .Build())])
                .Build();
            await context.Set<Board>().AddAsync(board, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
