var builder = DistributedApplication.CreateBuilder(args);
var isPublishing = builder.ExecutionContext.IsPublishMode;

var containerName = "mounthypython-" + (builder.Configuration["ENVIRONMENT"] == "Production" ? "prod" : "dev");
var aca = builder.AddAzureContainerAppEnvironment(containerName);

var postgres = builder.AddAzurePostgresFlexibleServer("postgres")
    .RunAsContainer(c =>
    {
        c.WithImageTag("18-alpine")
            .WithHostPort(5432)
            .WithDataVolume("mounthypython-postgres-data")
            .WithLifetime(ContainerLifetime.Persistent)
            .WithContainerName("mounthypython-postgres-container")
            .WithPgAdmin(pgAdmin =>
            {
                pgAdmin.WithHostPort(5050);
                pgAdmin.WithLifetime(ContainerLifetime.Persistent);
                pgAdmin.WithContainerName("mounthypython-pgadmin-container");
            });
    });

if (isPublishing)
{
    postgres.WithPasswordAuthentication();
}

var messages = builder.AddRabbitMQ("rabbit")
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithContainerName("mounthypython-rabbit-container")
    .PublishAsContainer();

var keycloakAdmin = builder.AddParameter("keycloak-admin", secret: false, value: "admin");
var keycloakPassword = builder.AddParameter("keycloak-password", secret: true, value: "admin");
int? keycloakPort = isPublishing ? null : 9001;

var keycloakDatabaseName = "mountypython_Keycloak" + (builder.Configuration["ENVIRONMENT"] == "Production" ? "" : "_DEV");
//configure keycloak +aspire + sqlserver: https://github.com/dotnet/aspire/discussions/6563
var keycloakDbResource = postgres.AddDatabase("mountypython-keycloakdb", keycloakDatabaseName);

var keycloakDbUrl = ReferenceExpression.Create($"jdbc:postgresql://{postgres.Resource.HostName}/{keycloakDbResource.Resource.DatabaseName}");
var keycloak = builder.AddKeycloak("mountypython-keycloak", adminUsername: keycloakAdmin, adminPassword: keycloakPassword, port: keycloakPort)
    .WithImageTag("26.8")
    .WithEnvironment("KC_DB", "postgres")
    .WithEnvironment("KC_DB_USERNAME", ReferenceExpression.Create($"{postgres!.Resource.UserName!}"))
    .WithEnvironment("KC_DB_PASSWORD", ReferenceExpression.Create($"{postgres!.Resource.Password!}"))
    .WithEnvironment("KC_DB_URL", keycloakDbUrl)
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    .WithEnvironment("KC_HTTP_ENABLED", "true")
    .WithEnvironment("KC_PROXY_HEADERS", "xforwarded")
    // OR: The only real suppression would be via Keycloak's --features-disabled flag — but the exact flag for the 3p-cookie check is not documented as a disableable feature in KC 26
    // for avoiding: issues: :9001/realms/mounytpython/protocol/openid-connect/3p-cookies/step1.html:1 requestStorageAccess: Must be handling a user gesture to use.
    .WithEnvironment("KC_SPI_STICKY_SESSION_ENCODER_INFINISPAN_SHOULD_ATTACH_ROUTE", "false")
    // Disable theme cache in dev so CSS/template changes are picked up on reload
    .WithEnvironment("KC_SPI_THEME_CACHE_THEMES", "false")
    .WithEndpoint("http", e =>
    {
        e.IsExternal = false;
        e.TargetPort = 8080; // Keycloak container listens on 8080
                             // In publish mode Keycloak sits behind ACA's HTTPS ingress;
                             // YARP must build https:// URLs to reach it.
                             // In dev it is plain HTTP on port 9001 (host-mapped).
        e.UriScheme = isPublishing ? "https" : "http";
    })
    .WaitFor(keycloakDbResource)
    .WithOtlpExporter()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithContainerName("mountypython-keycloak-container")
    .PublishAsContainer();

if (!isPublishing)
{
    keycloak.WithDataVolume()
        .WithRealmImport("./import.json")
        // Mount the custom mountypython login theme into the Keycloak themes directory
        .WithBindMount("./keycloak/themes/mountypython", "/opt/keycloak/themes/mountypython", isReadOnly: true);
}

var keycloakAuthority = ReferenceExpression.Create(
    $"{keycloak.GetEndpoint("http").Property(EndpointProperty.Url)}/realms/mountypython");

// Must match the audience mapper on the realm client in import.json.
const string keycloakAudience = "mountypython-api";

var notifications = builder.AddProject<Projects.Notification_Services>("notification-services")
    .WithEnvironment("Auth__Authority", keycloakAuthority)
    .WithEnvironment("Auth__Audience", keycloakAudience)
    .WithReference(messages).WaitFor(messages)
    .WithReference(keycloak).WaitFor(keycloak);

var inventoryDatabaseName = "MountyPython_Inventory" + (builder.Configuration["ENVIRONMENT"] == "Production" ? "" : "_DEV");
var inventoryDbResource = postgres.AddDatabase("mountypython-inventorydb", inventoryDatabaseName);
var inventory = builder.AddProject<Projects.Inventory_Services>("inventory-services")
    .WithEnvironment("Auth__Authority", keycloakAuthority)
    .WithEnvironment("Auth__Audience", keycloakAudience)
    .WithReference(inventoryDbResource).WaitFor(inventoryDbResource)
    .WithReference(notifications).WaitFor(notifications)
    .WithReference(messages).WaitFor(messages)
    .WithReference(keycloak).WaitFor(keycloak);

var operativeDatabaseName = "MountyPython_Operative" + (builder.Configuration["ENVIRONMENT"] == "Production" ? "" : "_DEV");
var operativeDbResource = postgres.AddDatabase("mountypython-operativedb", operativeDatabaseName);
var orders = builder.AddProject<Projects.Operative_Services>("operative-services")
    .WithEnvironment("Auth__Authority", keycloakAuthority)
    .WithEnvironment("Auth__Audience", keycloakAudience)
    .WithReference(operativeDbResource).WaitFor(operativeDbResource)
    .WithReference(notifications).WaitFor(notifications)
    .WithReference(messages).WaitFor(messages)
    .WithReference(keycloak).WaitFor(keycloak);

// Another try of BFF with Yarp and Angular inspired from here:
// https://juliocasal.com/blog/how-to-deploy-a-net-react-full-stack-app-to-azure-with-aspire-13
var webApp = builder.AddJavaScriptApp("mountypython-client", "../Clients/mountypython.client", runScriptName: "start")
    .WithEnvironment("DEV_SERVER_PORT", "4200")
    .WithNpm(install: true, installCommand: "install", installArgs: ["--legacy-peer-deps"])
    .WithReference(inventory).WaitFor(inventory)
    .WithReference(orders).WaitFor(orders)
    .WithReference(notifications).WaitFor(notifications)
    .WithReference(keycloak).WaitFor(keycloak)
    .WithBuildScript("build")
    .WithEndpoint(endpointName: "http", endpoint =>
    {
        endpoint.UriScheme = "http";
        endpoint.TargetPort = isPublishing ? null : 4200;
    });

if (isPublishing)
{
    var yarp = builder.AddYarp("frontend-proxy")
        .WithReference(keycloak).WaitFor(keycloak)
        .WithReference(inventory).WaitFor(inventory)
        .WithReference(orders).WaitFor(orders)
        .WithReference(notifications).WaitFor(notifications)
        .WithConfiguration(config =>
        {
            config.AddRoute("/auth/{**catch-all}", keycloak);
            config.AddRoute("/resources/{**catch-all}", keycloak);
            config.AddRoute("/realms/{**catch-all}", keycloak);
            config.AddRoute("/js/{**catch-all}", keycloak);
            config.AddRoute("/themes/{**catch-all}", keycloak);
            config.AddRoute("/admin/{**catch-all}", keycloak);

            config.AddRoute("/api/catalogs/{**catch-all}", inventory);
            config.AddRoute("/api/opertions/{**catch-all}", orders);

            config.AddRoute("/api/notifications/{**catch-all}", notifications);
            config.AddRoute("/api/v1/updateshub/{**catch-all}", notifications);
        })
        //.WithExternalHttpEndpoints()
        .PublishWithStaticFiles(webApp);
}

builder.Build().Run();
