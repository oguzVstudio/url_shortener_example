using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Infrastructure.Consumers;
using UrlShortener.Infrastructure.Publishers;

namespace UrlShortener.Infrastructure.Shared.Extensions;

public static partial class RegistrationExtensions
{
    public static IServiceCollection AddMasstransitInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCustomInMemoryMassTransit(
            configuration,
            (context, cfg) =>
            {
                cfg.AddUrlTrackingConsumer(context);

                cfg.AddShortenedUrlTrackPublisher();
            },
            configureBusRegistration: x =>
            {
                x.AddConsumer<UrlTrackingConsumer>();
            });

        return services;
    }
}
