using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Shorten.ShortenUrls;

namespace UrlShortener.Domain;

public interface IShortenDbContext
{
    DbSet<ShortenUrl> ShortenUrls { get; }
    DbSet<ShortenUrlTrack> ShortenUrlTracks { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}