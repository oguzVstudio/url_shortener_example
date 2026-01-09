using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using UrlShortener.Application.Features.Shared.Cache;
using UrlShortener.Application.Features.Shorten.Services.v1.Models;
using UrlShortener.Application.Services.CodeGenerators;
using UrlShortener.Application.Shared.Cache;
using UrlShortener.Domain;
using UrlShortener.Domain.Shorten.Settings;
using UrlShortener.Domain.Shorten.ShortenUrls;

namespace UrlShortener.Application.Features.Shorten.Services.v1;

public class ShortenUrlAppService : IShortenUrlAppService
{
    private readonly IShortenDbContext _context;
    private readonly HybridCache _hybridCache;
    private readonly IDistributedLock _distributedLock;
    private readonly IUniqueCodeGenerator _uniqueCodeGenerator;
    private readonly ShortenUrlSettings _shortenUrlSettings;

    private const string ShortUrlCacheKey = "shortUrl:{0}";
    private const string ShortUrlCodeLockKey = "shortUrlCodeLock:{0}";

    public ShortenUrlAppService(IShortenDbContext context,
        HybridCache hybridCache,
        IDistributedLock distributedLock,
        IUniqueCodeGenerator uniqueCodeGenerator,
        IOptions<ShortenUrlSettings> shortenUrlSettings)
    {
        _context = context;
        _hybridCache = hybridCache;
        _distributedLock = distributedLock;
        _uniqueCodeGenerator = uniqueCodeGenerator;
        _shortenUrlSettings = shortenUrlSettings.Value;
    }

    public async Task<CreateShortUrlResponse> ShortenUrlAsync(CreateShortUrlRequest request,
        CancellationToken cancellationToken)
    {
        var code = await GenerateCodeAsync(cancellationToken);
        var shortUrl = $"{_shortenUrlSettings.BaseUrl}/{code}";

        var shortenUrl = ShortenUrl.Create(request.Url,
            shortUrl,
            code,
            request.IsExpiring,
            request.ExpiresAt);

        await _context.ShortenUrls.AddAsync(shortenUrl, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await _distributedLock.TryRemoveAsync(
            string.Format(ShortUrlCodeLockKey, shortenUrl.Code),
            cancellationToken);
        return new CreateShortUrlResponse(shortUrl, code, true);
    }

    public async Task<GetOriginalUrlResponse> GetOriginalUrlAsync(string code, CancellationToken cancellationToken)
    {
        var cacheKey = new ShortenDistributedCacheKey(string.Format(ShortUrlCacheKey, code));

        var originalUrl = await _hybridCache.GetOrCreateAsync<string>(
            cacheKey.Key,
            async entry =>
            {
                var shortenedUrl = await _context.ShortenUrls
                    .Where(x => x.Code == code
                                && (!x.IsExpiring || x.ExpiresAt > DateTimeOffset.UtcNow))
                    .Select(x => new
                    {
                        x.LongUrl,
                        x.IsExpiring,
                        x.ExpiresAt
                    }).FirstOrDefaultAsync(entry);

                var url = shortenedUrl?.LongUrl ?? string.Empty;
                return url;
            }, cancellationToken: cancellationToken);
        return new GetOriginalUrlResponse(originalUrl,
            !string.IsNullOrWhiteSpace(originalUrl));
    }

    public async Task<bool> TrackUrlAccessAsync(CreateShortUrlTrackRequest request, CancellationToken cancellationToken)
    {
        var shortenUrl = await _context.ShortenUrls
            .FirstOrDefaultAsync(x => x.Code == request.Code, cancellationToken);

        if (shortenUrl is null)
        {
            return false;
        }

        shortenUrl.IncrementAttemptCount();

        var shortenUrlTrack = ShortenUrlTrack.Create(shortenUrl.Id,
            shortenUrl.Code,
            request.IpAddress,
            request.UserAgent,
            request.AccessedAt);

        await _context.ShortenUrlTracks.AddAsync(shortenUrlTrack, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<string> GenerateCodeAsync(CancellationToken cancellationToken)
    {
        string code;
        bool generated;
        do
        {
            code = await _uniqueCodeGenerator.GenerateAsync(cancellationToken);
            var isLocked = await _distributedLock.TryLockAsync(
                string.Format(ShortUrlCodeLockKey, code),
                TimeSpan.FromMinutes(10),
                cancellationToken);

            if (isLocked)
            {
                var exists = await _context.ShortenUrls.AnyAsync(x => x.Code == code, cancellationToken);
                generated = !exists;
            }
            else
            {
                generated = false;
            }
        } while (!generated);

        return code;
    }
}