using Commons.Domain;
using Commons.Services;
using Operative.Contracts;
using Operative.Services.Features.Operations.Domain;

namespace Operative.Services.Features.Operations.Queries;

/// <summary>Serializes an order to JSON for production download/interoperability (see README).</summary>
public interface IDownloadOrderQuery
{
    Task<OrderDownloadDTO> ExecuteAsync(CurrentUser currentUser, string orderId, CancellationToken cancellationToken);
}

public class DownloadOrderQuery(IOrderRepository repository) : IDownloadOrderQuery
{
    public async Task<OrderDownloadDTO> ExecuteAsync(CurrentUser currentUser, string orderId, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIDWithDetailsAsync(orderId, cancellationToken)
            ?? throw new NotFoundException($"Order '{orderId}' was not found.");

        if (!currentUser.isManager && order.CustomerID != currentUser.userId)
        {
            throw new NotFoundException($"Order '{orderId}' was not found.");
        }

        var json = System.Text.Json.JsonSerializer.Serialize(order.ToDTO());
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        return new OrderDownloadDTO($"order-{order.ID}.json", "application/json", bytes);
    }
}
