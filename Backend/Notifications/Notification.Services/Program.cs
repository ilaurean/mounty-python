using Commons.Services;
using Notification.Services.Infrastructure.Messaging;
using Rebus.Config;
using Rebus.Routing.TypeBased;

namespace Notification.Services;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        builder.SetupLogging();

        // Add services to the container.
        builder.Services.AddAuthentication().AddJwtBearer();
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddSignalR();
        builder.Services.AddTransient<IPushNotificationService, PushNotificationService>();

        var rabbitConn = builder.Configuration.GetConnectionString("rabbit");
        builder.Services.AddRebus(cfg => cfg
            .Transport(t => t.UseRabbitMq(rabbitConn, "notification-services"))
            .Routing(r => r.TypeBased()));
        builder.Services.AutoRegisterHandlersFromAssemblyOf<Program>();

        var app = builder.Build();
        app.UseMiddleware<LoggingMiddleware>();
        app.UseCustomExceptionHandler();

        app.MapDefaultEndpoints();
        app.MapDevelopmentEndpoints("Notification API v1.0");

        app.MapHub<MainHub>("/api/v1/updateshub");

        app.Run();
    }
}
