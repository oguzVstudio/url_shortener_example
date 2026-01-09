using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace UrlShortener.Infrastructure.Context;

public class ShortenDbContextDesignFactory : IDesignTimeDbContextFactory<ShortenDbContext>
{
    private readonly string _migrationDefaultSchema = "public";
    private readonly string _connectionStringSection = "PostgresOptions:ConnectionString";

    public ShortenDbContext CreateDbContext(string[] args)
    {
        var builder = GetConfigurationBuilder(args);
        var configuration = builder.Build();
        var connectionStringSectionValue = configuration.GetValue<string>(_connectionStringSection);

        if (string.IsNullOrWhiteSpace(connectionStringSectionValue))
        {
            throw new InvalidOperationException($"Could not find a value for {_connectionStringSection} section.");
        }

        var optionsBuilder = NpgsqlDbContextOptionsBuilderExtensions.UseNpgsql((DbContextOptionsBuilder)new DbContextOptionsBuilder<ShortenDbContext>(), connectionStringSectionValue,
                 sqlOptions =>
                 {
                     sqlOptions.MigrationsAssembly(GetType().Assembly.FullName);
                     sqlOptions.MigrationsHistoryTable(
                                 $"efcore_{_migrationDefaultSchema}_migration_history", _migrationDefaultSchema);
                     sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                     sqlOptions.CommandTimeout(180);
                 }
             );

        return (ShortenDbContext)Activator.CreateInstance(typeof(ShortenDbContext), optionsBuilder.Options)!;
    }

    public virtual IConfigurationBuilder GetConfigurationBuilder(string[] args)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory ?? "")
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environmentName}.json", true)
            .AddEnvironmentVariables();

        Console.WriteLine(environmentName);
        return builder;
    }
}
