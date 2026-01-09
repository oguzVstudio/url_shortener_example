using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Shorten.ShortenUrls;
using UrlShortener.Infrastructure.Context;

namespace UrlShortener.Infrastructure.ModelConfigurations.Shorten;

public class ShortenedUrlEntityConfiguration : IEntityTypeConfiguration<ShortenUrl>
{
    public void Configure(EntityTypeBuilder<ShortenUrl> builder)
    {
        builder.ToTable("shorten_urls", ShortenDbContext.DefaultSchema);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Id).IsUnique();

        builder
            .Property(shortenedUrl => shortenedUrl.Code)
            .HasMaxLength(10);

        builder
            .HasIndex(shortenedUrl => shortenedUrl.Code)
            .IsUnique();

        builder.Property(x => x.LongUrl)
            .HasMaxLength(2048);

        builder.Property(x => x.ShortUrl)
            .HasMaxLength(512);

        builder.Property(x => x.CreatedOnUtc)
          .HasColumnType("timestamptz")
          .HasConversion(
              v => v,
              v => v.ToUniversalTime()
          );

        builder.Property(x => x.ExpiresAt)
          .HasColumnType("timestamptz")
          .HasConversion(
              v => v,
              v => v.HasValue ? v.Value.ToUniversalTime() : null
          );
    }
}
