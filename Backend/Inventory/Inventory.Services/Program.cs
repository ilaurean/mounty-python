using Commons.Infrastructure.Messaging;
using Commons.Infrastructure.Repositories;
using Commons.Services;
using FluentValidation;
using Inventory.Services.Features.Catalogs;
using Inventory.Services.Infrastructure.Repositories;
using Rebus.Config;
using Rebus.Routing.TypeBased;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.SetupLogging();

builder.SetupNpgsqlServerDbContext<InventoryDbContext>("mountypython-inventorydb");

builder.Services.SetupJsonDefaults();

// Add services to the container.
builder.Services.AddMountyJwtBearer(builder.Configuration, builder.Environment);
builder.Services.AddMountyAuthorization();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddCatalogsServices();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var rabbitConn = builder.Configuration.GetConnectionString("rabbit");
builder.Services.AddRebus(cfg => cfg
    .Transport(t => t.UseRabbitMq(rabbitConn, "inventory-services"))
    .Routing(r => r.TypeBased()));
builder.Services.AutoRegisterHandlersFromAssemblyOf<Program>();
builder.Services.AddScoped<IPublisher, Publisher>();

builder.Services.AddHostedService<MigratorHostedService<InventoryDbContext>>();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<InventorySeedHostedService>();
}
//builder.Services.AddHostedService<InventoryWarmupHostedService>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapDevelopmentEndpoints("Inventory Services v1.0");

app.UseAuthentication();
app.UseAuthorization();

app.MapCatalogsEndpoints();

app.Run();


namespace Inventory.Services
{
    public partial class Program { }
}
