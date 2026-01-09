using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain;
using UrlShortener.Domain.Shorten.ShortenUrls;
using UrlShortener.Infrastructure.Extensions;

namespace UrlShortener.Infrastructure.Context;

public class ShortenDbContext : DbContext, IShortenDbContext
{
    public const string DefaultSchema = "shorten";
    public ShortenDbContext(DbContextOptions<ShortenDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.ApplySnakeCaseNamingConvention();
    }

    public DbSet<ShortenUrl> ShortenUrls => Set<ShortenUrl>();

    public DbSet<ShortenUrlTrack> ShortenUrlTracks => Set<ShortenUrlTrack>();
}