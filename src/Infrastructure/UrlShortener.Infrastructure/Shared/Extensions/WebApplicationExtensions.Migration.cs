using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UrlShortener.Infrastructure.Context;

namespace UrlShortener.Infrastructure.Shared.Extensions;

public static partial class WebApplicationExtensions
{
    public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        var configuration = app.Services.GetRequiredService<IConfiguration>();

        if (configuration.GetValue<bool>("PostgresOptions:UseInMemory") == false)
        {
            using var serviceScope = app.Services.CreateScope();
            var locationDbContext = serviceScope.ServiceProvider.GetRequiredService<ShortenDbContext>();

            app.Logger.LogInformation("Updating database...");

            await locationDbContext.Database.MigrateAsync();

            app.Logger.LogInformation("Updated database");
        }
    }
}
