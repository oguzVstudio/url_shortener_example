using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UrlShortener.Domain;
using UrlShortener.Infrastructure.Context;
using UrlShortener.Infrastructure.Settings;

namespace UrlShortener.Infrastructure.Shared.Extensions;

public static partial class RegistrationExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddOptions<PostgresOptions>().BindConfiguration(nameof(PostgresOptions));
        services.AddSingleton(x => x.GetRequiredService<IOptions<PostgresOptions>>().Value);

        services.AddDbContext<ShortenDbContext>((sp, options) =>
        {
            var postgresOptions = sp.GetRequiredService<PostgresOptions>();
            options.UseNpgsql(postgresOptions.ConnectionString,
                sqlOptions =>
                {
                    var name = postgresOptions.MigrationAssembly ?? typeof(ShortenDbContext).Assembly.GetName().Name;

                    sqlOptions.MigrationsAssembly(name);
                    sqlOptions.MigrationsHistoryTable(
                        $"efcore_public_migration_history", "public");
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                });
        });
        services.AddScoped<IShortenDbContext>(provider => provider.GetRequiredService<ShortenDbContext>());
        return services;
    }
}
