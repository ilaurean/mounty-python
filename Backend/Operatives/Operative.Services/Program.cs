using Commons.Infrastructure.Repositories;
using Commons.Services;
using FluentValidation;
using Operative.Services.Features.Operations;
using Operative.Services.Infrastructure.Repositories;
using Rebus.Config;
using Rebus.Routing.TypeBased;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.SetupLogging();

builder.SetupNpgsqlServerDbContext<OperativeDbContext>("mountypython-operativedb");

// Add services to the container.
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var rabbitConn = builder.Configuration.GetConnectionString("rabbit");
builder.Services.AddRebus(cfg => cfg
    .Transport(t => t.UseRabbitMq(rabbitConn, "operative-services"))
    .Routing(r => r.TypeBased()));
builder.Services.AutoRegisterHandlersFromAssemblyOf<Program>();

builder.Services.AddHostedService<MigratorHostedService<OperativeDbContext>>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapDevelopmentEndpoints("Operative Services v1.0");

app.UseAuthentication();
app.UseAuthorization();

app.MapOperationsEndpoints();

app.Run();


namespace Operative.Services
{
    public partial class Program { }
}