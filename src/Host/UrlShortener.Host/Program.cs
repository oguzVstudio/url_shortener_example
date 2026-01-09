using System.Reflection;
using UrlShortener.Host.Extensions;
using UrlShortener.Host.Features.Shorten.UrlRedirection;
using UrlShortener.Infrastructure.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointModules(Assembly.GetExecutingAssembly());

builder.Services
    .AddDomain(builder.Configuration)
    .AddApplication(builder.Configuration)
    .AddPersistence(builder.Configuration)
    .AddMasstransitInfrastructure(builder.Configuration)
    .AddCache(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapRedirectOriginalUrlEndpoint();

app.MapEndpointModules();

app.Run();