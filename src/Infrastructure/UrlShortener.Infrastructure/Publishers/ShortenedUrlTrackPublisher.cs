using MassTransit;
using UrlShortener.Infrastructure.Shared.Events;

namespace UrlShortener.Infrastructure.Publishers;

public static class ShortenedUrlTrackPublisher
{
    public static void AddShortenedUrlTrackPublisher(this IInMemoryBusFactoryConfigurator cfg)
    {
        cfg.Message<UrlTrackingEvent>(x =>
        {
            x.SetEntityName("url_tracking_event.input_exchange");
        });

        cfg.Publish<UrlTrackingEvent>(x =>
        {
            x.ExchangeType = MassTransit.Transports.Fabric.ExchangeType.FanOut;
        });
    }
}
