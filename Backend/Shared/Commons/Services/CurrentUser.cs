using System.Security.Claims;

namespace Commons.Services;

/// <summary>
/// The authenticated caller, derived from the Keycloak access token claims.
/// </summary>
public sealed record CurrentUser(string userId, bool isManager)
{
    public const string ManagerRole = "manager";
    public const string CustomerRole = "customer";

    public static CurrentUser From(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var userId = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("The access token has no subject claim.");

        return new CurrentUser(userId, principal.IsInRole(ManagerRole));
    }

    /// <summary>Throws when the current user is not a manager.</summary>
    public void EnsureManager()
    {
        if (!isManager)
        {
            throw new UnauthorizedAccessException("Only managers can perform this operation.");
        }
    }
}
