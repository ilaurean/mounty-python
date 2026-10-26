using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;
using System.Text.Json;

namespace Commons.Services;

public static class SecurityExtensions
{
    public const string ManagerPolicy = "Manager";
    public const string CustomerPolicy = "Customer";
    public const string CustomerOrManagerPolicy = "CustomerOrManager";

    /// <summary>
    /// Configures JWT bearer authentication against Keycloak using Auth:Authority (Auth__Authority) and Auth:Audience.
    /// HTTP metadata is only allowed in the Development environment.
    /// </summary>
    public static IServiceCollection AddMountyJwtBearer(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var authority = configuration["Auth:Authority"];
        if (string.IsNullOrWhiteSpace(authority))
        {
            throw new InvalidOperationException("Configuration 'Auth:Authority' (Auth__Authority) is required.");
        }

        var audience = configuration["Auth:Audience"];
        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException("Configuration 'Auth:Audience' (Auth__Audience) is required.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.RequireHttpsMetadata = !environment.IsDevelopment();
                options.MapInboundClaims = false;
                options.TokenValidationParameters.ValidateIssuer = true;
                options.TokenValidationParameters.ValidateAudience = true;
                options.TokenValidationParameters.NameClaimType = "preferred_username";
                options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;
            });
        return services;
    }

    /// <summary>
    /// Registers the Keycloak realm-role claims mapping
    /// </summary>
    public static IServiceCollection AddMountyAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IClaimsTransformation, KeycloakRealmRolesClaimsTransformation>();
        services.AddAuthorization(options =>
        {
            options.AddPolicy(ManagerPolicy, p => p.RequireAuthenticatedUser().RequireRole(CurrentUser.ManagerRole));
            options.AddPolicy(CustomerPolicy, p => p.RequireAuthenticatedUser().RequireRole(CurrentUser.CustomerRole));
            options.AddPolicy(CustomerOrManagerPolicy, p => p.RequireAuthenticatedUser()
                .RequireRole(CurrentUser.CustomerRole, CurrentUser.ManagerRole));
        });
        return services;
    }
}

/// <summary>Maps Keycloak's nested realm_access.roles claim to standard role claims.</summary>
public sealed class KeycloakRealmRolesClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.Identity is not ClaimsIdentity identity || principal.FindFirst("realm_access") is not { } claim)
        {
            return Task.FromResult(principal);
        }

        using var document = JsonDocument.Parse(claim.Value);
        if (document.RootElement.TryGetProperty("roles", out var roles) && roles.ValueKind == JsonValueKind.Array)
        {
            foreach (var role in roles.EnumerateArray().Select(r => r.GetString()).OfType<string>())
            {
                if (!identity.HasClaim(ClaimTypes.Role, role))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }
        }

        return Task.FromResult(principal);
    }
}
