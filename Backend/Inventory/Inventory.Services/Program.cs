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

// Add services to the container.
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var rabbitConn = builder.Configuration.GetConnectionString("rabbit");
builder.Services.AddRebus(cfg => cfg
    .Transport(t => t.UseRabbitMq(rabbitConn, "inventory-services"))
    .Routing(r => r.TypeBased()));
builder.Services.AutoRegisterHandlersFromAssemblyOf<Program>();

builder.Services.AddHostedService<MigratorHostedService<InventoryDbContext>>();

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
