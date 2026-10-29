using Commons.Services;
using Microsoft.AspNetCore.Mvc;
using Operative.Contracts;
using Operative.Services.Features.Operations.Commands;
using Operative.Services.Features.Operations.Domain;
using Operative.Services.Features.Operations.Queries;

namespace Operative.Services.Features.Operations;

public static class OperationsModule
{
    public static IServiceCollection AddOperationsServices(this IServiceCollection services)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Queries
        services.AddScoped<IGetOrderByIDQuery, GetOrderByIDQuery>();
        services.AddScoped<ISearchOrdersQuery, SearchOrdersQuery>();
        services.AddScoped<IDownloadOrderQuery, DownloadOrderQuery>();

        // Commands
        services.AddScoped<ICreateOrderCommand, CreateOrderCommand>();
        services.AddScoped<IUpdateOrderCommand, UpdateOrderCommand>();
        services.AddScoped<IStartOrderProcessingCommand, StartOrderProcessingCommand>();
        services.AddScoped<ICompleteOrderCommand, CompleteOrderCommand>();
        services.AddScoped<ICancelOrderCommand, CancelOrderCommand>();

        return services;
    }

    public static IEndpointRouteBuilder MapOperationsEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/operations").WithTags("Operations")
            .RequireAuthorization(SecurityExtensions.CustomerOrManagerPolicy);

        group.MapPost("/orders", async (
            [FromBody] CreateOrderRequest request,
            ICreateOrderCommand command,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var currentUser = CurrentUser.From(httpContext.User);
            var order = await command.ExecuteAsync(currentUser, request, cancellationToken);
            return Results.Created($"/api/operations/orders/{order.ID}", order.ToDTO());
        }).RequireAuthorization(SecurityExtensions.CustomerPolicy);

        group.MapGet("/orders/{orderId}", async (
            string orderId,
            IGetOrderByIDQuery query,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var currentUser = CurrentUser.From(httpContext.User);
            var dto = await query.ExecuteAsync(currentUser, orderId, cancellationToken);
            return Results.Ok(dto);
        });

        group.MapGet("/orders", async (
            [AsParameters] OrderFilterDTO filter,
            ISearchOrdersQuery query,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var currentUser = CurrentUser.From(httpContext.User);
            var results = await query.ExecuteAsync(currentUser, filter, cancellationToken);
            return Results.Ok(results);
        });

        group.MapPut("/orders/{orderId}", async (
            string orderId,
            [FromBody] UpdateOrderRequest request,
            IUpdateOrderCommand command,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var currentUser = CurrentUser.From(httpContext.User);
            var order = await command.ExecuteAsync(currentUser, orderId, request, cancellationToken);
            return Results.Ok(order.ToDTO());
        });

        group.MapPost("/orders/{orderId}/start-processing", async (
            string orderId,
            IStartOrderProcessingCommand command,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var currentUser = CurrentUser.From(httpContext.User);
            var order = await command.ExecuteAsync(currentUser, orderId, cancellationToken);
            return Results.Ok(order.ToDTO());
        }).RequireAuthorization(SecurityExtensions.ManagerPolicy);

        group.MapPost("/orders/{orderId}/complete", async (
            string orderId,
            ICompleteOrderCommand command,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var currentUser = CurrentUser.From(httpContext.User);
            var order = await command.ExecuteAsync(currentUser, orderId, cancellationToken);
            return Results.Ok(order.ToDTO());
        }).RequireAuthorization(SecurityExtensions.ManagerPolicy);

        group.MapPost("/orders/{orderId}/cancel", async (
            string orderId,
            ICancelOrderCommand command,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var currentUser = CurrentUser.From(httpContext.User);
            var order = await command.ExecuteAsync(currentUser, orderId, cancellationToken);
            return Results.Ok(order.ToDTO());
        });

        group.MapGet("/orders/{orderId}/download", async (
            string orderId,
            IDownloadOrderQuery query,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var currentUser = CurrentUser.From(httpContext.User);
            var download = await query.ExecuteAsync(currentUser, orderId, cancellationToken);
            return Results.File(download.Content, download.ContentType, download.FileName);
        });

        return builder;
    }
}
