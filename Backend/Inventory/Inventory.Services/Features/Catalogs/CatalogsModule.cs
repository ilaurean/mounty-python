using Commons.Services;
using Inventory.Contracts;
using Inventory.Services.Features.Catalogs.Commands;
using Inventory.Services.Features.Catalogs.Domain;
using Inventory.Services.Features.Catalogs.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Services.Features.Catalogs;

public static class CatalogsModule
{
    public static IServiceCollection AddCatalogsServices(this IServiceCollection services)
    {
        services.AddScoped<ICatalogRepository, CatalogRepository>();

        // Queries
        services.AddTransient<IGetBoardByIDQuery, GetBoardByIDQuery>();
        services.AddTransient<ISearchBoardsQuery, SearchBoardsQuery>();

        // Commands
        services.AddTransient<ICreateBoardCommand, CreateBoardCommand>();
        services.AddTransient<IUpdateBoardCommand, UpdateBoardCommand>();
        services.AddTransient<IDeleteBoardCommand, DeleteBoardCommand>();

        return services;
    }

    public static IEndpointRouteBuilder MapCatalogsEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/catalogs").WithTags("Catalogs").RequireAuthorization(SecurityExtensions.CustomerOrManagerPolicy);

        group.MapGet("/boards", async ([AsParameters] BoardFilterDTO filter, ISearchBoardsQuery query, HttpContext http, CancellationToken ct) =>
            Results.Ok(await query.ExecuteAsync(CurrentUser.From(http.User), filter, ct)));

        group.MapGet("/boards/{boardId}", async (string boardId, IGetBoardByIDQuery query, HttpContext http, CancellationToken ct) =>
            Results.Ok(await query.ExecuteAsync(CurrentUser.From(http.User), boardId, ct)));

        group.MapPost("/boards", async ([FromBody] CreateBoardRequest request, ICreateBoardCommand command, HttpContext http, CancellationToken ct) =>
        {
            var board = await command.ExecuteAsync(CurrentUser.From(http.User), request, ct);
            return Results.Created($"/api/catalogs/boards/{board.ID}", board.ToDTO());
        }).RequireAuthorization(SecurityExtensions.ManagerPolicy);

        group.MapPut("/boards/{boardId}", async (string boardId, [FromBody] UpdateBoardRequest request, IUpdateBoardCommand command, HttpContext http, CancellationToken ct) =>
            Results.Ok((await command.ExecuteAsync(CurrentUser.From(http.User), boardId, request, ct)).ToDTO()))
            .RequireAuthorization(SecurityExtensions.ManagerPolicy);

        group.MapDelete("/boards/{boardId}", async (string boardId, IDeleteBoardCommand command, HttpContext http, CancellationToken ct) =>
        {
            await command.ExecuteAsync(CurrentUser.From(http.User), boardId, ct);
            return Results.NoContent();
        }).RequireAuthorization(SecurityExtensions.ManagerPolicy);

        return builder;
    }
}
