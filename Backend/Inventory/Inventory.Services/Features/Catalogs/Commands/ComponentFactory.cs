using Inventory.Contracts;
using Inventory.Services.Features.Catalogs.Domain;

namespace Inventory.Services.Features.Catalogs.Commands;

internal static class ComponentFactory
{
    public static IList<Component> Build(IEnumerable<ComponentRequest> requests) =>
        requests.Select(c => new Component.ComponentBuilder()
            .WithName(c.Name)
            .WithDescription(c.Description!)
            .WithQuantity(c.Quantity)
            .Build()).ToList();
}
