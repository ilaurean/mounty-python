namespace Inventory.Services.Features.Catalogs;

public static class CatalogsModule
{
    public static IEndpointRouteBuilder MapCatalogsEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/catalogs").WithTags("Catalogs");
        return builder;
    }
}
