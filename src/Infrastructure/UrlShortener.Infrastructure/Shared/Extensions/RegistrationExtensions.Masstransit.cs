using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Infrastructure.Bus.Masstransit;

namespace UrlShortener.Infrastructure.Shared.Extensions;

public static partial class RegistrationExtensions
{
    public static IServiceCollection AddCustomInMemoryMassTransit(this IServiceCollection services, 
        IConfiguration configuration,
        Action<IBusRegistrationContext, IInMemoryBusFactoryConfigurator>? configureReceiveEndpoints = null,
        Action<IBusRegistrationConfigurator>? configureBusRegistration = null)
    {
        services.AddMassTransit(ConfiguratorAction);

        void ConfiguratorAction(IBusRegistrationConfigurator busRegistrationConfigurator)
        {
            configureBusRegistration?.Invoke(busRegistrationConfigurator);
            busRegistrationConfigurator.AddDelayedMessageScheduler();
            //busRegistrationConfigurator.AddConsumers(assemblies);
            busRegistrationConfigurator.SetEndpointNameFormatter(new SnakeCaseEndpointNameFormatter(false));

            busRegistrationConfigurator.UsingInMemory((context, cfg) =>
            {
                cfg.UseDelayedMessageScheduler();
                cfg.UseMessageRetry(r => AddRetryConfiguration(r));

                configureReceiveEndpoints?.Invoke(context, cfg);
            });
        }

        services.AddTransient<Application.Shared.Bus.IBus, MasstransitBus>();

        return services;
    }

    private static IRetryConfigurator AddRetryConfiguration(IRetryConfigurator retryConfigurator)
    {
        retryConfigurator
            .Exponential(3, TimeSpan.FromMilliseconds(200), TimeSpan.FromMinutes(120), TimeSpan.FromMilliseconds(200));

        return retryConfigurator;
    }
}
