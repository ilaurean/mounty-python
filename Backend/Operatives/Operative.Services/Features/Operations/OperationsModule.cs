namespace Operative.Services.Features.Operations;

public static class OperationsModule
{
    public static IEndpointRouteBuilder MapOperationsEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/operations").WithTags("Operations");
        return builder;
    }
}
