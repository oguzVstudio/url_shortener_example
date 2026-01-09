using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
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
    .AddCache(builder.Configuration)
    .AddDistributedLock(builder.Configuration);

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 5;// 5 requests
        opt.Window = TimeSpan.FromSeconds(10); // 10 seconds
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0; // No queue
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRateLimiter();

app.MapRedirectOriginalUrlEndpoint();

app.MapEndpointModules();

app.Run();