using Commons.Services;
using Notification.Services.Infrastructure.Messaging;
using Rebus.Config;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.SetupLogging();

builder.Services.SetupJsonDefaults();

// Add services to the container.
builder.Services.AddMountyJwtBearer(builder.Configuration, builder.Environment);
builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddTransient<IPushNotificationService, PushNotificationService>();

var rabbitConn = builder.Configuration.GetConnectionString("rabbit");
builder.Services.AddRebus(cfg => cfg
    .Transport(t => t.UseRabbitMq(rabbitConn, "notification-services"))
    .Options(o => o.RetryStrategy(
        maxDeliveryAttempts: 5,
        errorQueueName: "notification-error-queue",
        secondLevelRetriesEnabled: true))  // Enable second-level retries
    .Routing(r => r.TypeBased()));
builder.Services.AutoRegisterHandlersFromAssemblyOf<Program>();

var app = builder.Build();
app.UseMiddleware<LoggingMiddleware>();
app.UseCustomExceptionHandler();

app.MapDefaultEndpoints();
app.MapDevelopmentEndpoints("Notification API v1.0");

app.MapHub<MainHub>("/api/v1/updateshub");

app.Run();


namespace Notification.Services
{
    public partial class Program { }
}