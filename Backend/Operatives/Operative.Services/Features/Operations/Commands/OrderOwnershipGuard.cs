using Commons.Domain;
using Commons.Services;

namespace Operative.Services.Features.Operations.Commands;

/// <summary>Ensures a customer can only act on their own orders; managers bypass this check.</summary>
internal static class OrderOwnershipGuard
{
    public static void EnsureAccess(CurrentUser currentUser, string customerId)
    {
        if (!currentUser.isManager && currentUser.userId != customerId)
        {
            throw new NotFoundException("Order was not found.");
        }
    }
}
