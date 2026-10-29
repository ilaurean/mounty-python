using Aspire.Hosting.Azure;

namespace Aspire.Hosting;

/// <summary>
/// Copied from here and modified to support Postgre Flex Server: https://github.com/CommunityToolkit/Aspire/blob/main/src/CommunityToolkit.Aspire.Hosting.Keycloak.Extensions/KeycloakPostgresExtension.cs
/// Provides extension methods for integrating Keycloak resources with PostgreSQL.
/// </summary>
public static class KeycloakPostgresExtension
{
    private static IResourceBuilder<KeycloakResource> WithPostgresData(
        this IResourceBuilder<KeycloakResource> builder,
        IResourceBuilder<AzurePostgresFlexibleServerDatabaseResource> database,
        bool xaEnabled = false)
    {
        var jdbcUrl = database.Resource.JdbcConnectionString;

        builder.WithEnvironment("KC_DB", "postgres")
            .WithEnvironment("KC_DB_URL", jdbcUrl)
            .WithEnvironment("KC_TRANSACTION_XA_ENABLED", xaEnabled.ToString().ToLower())
            .WaitFor(database);

        return builder;
    }

    /// Adds support for Keycloak to connect to a specified Postgres database resource, with optional credentials
    /// and configuration for XA transactions.
    /// <param name="builder">
    /// The resource builder for configuring a Keycloak resource.
    /// </param>
    /// <param name="database">
    /// The resource builder for the Postgres database that Keycloak will connect to.
    /// </param>
    /// <param name="xaEnabled">
    /// A boolean flag indicating whether XA transactions are enabled. Defaults to false.
    /// </param>
    /// <returns>
    /// An updated resource builder with Postgres integration configured for the Keycloak resource.
    /// </returns>
    [AspireExport]
    public static IResourceBuilder<KeycloakResource> WithAzurePostgresFlexibleServer(
        this IResourceBuilder<KeycloakResource> builder,
        IResourceBuilder<AzurePostgresFlexibleServerDatabaseResource> database, bool xaEnabled = false)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(database);

        return WithPostgresData(builder, database, xaEnabled)
            .WithEnvironment("KC_DB_USERNAME", database.Resource.Parent.UserName!)
            .WithEnvironment("KC_DB_PASSWORD", database.Resource.Parent.Password!);
    }
}
