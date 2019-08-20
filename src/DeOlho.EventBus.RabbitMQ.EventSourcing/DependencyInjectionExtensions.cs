using DeOlho.EventBus.Abstractions;
using DeOlho.EventBus.RabbitMQ.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DeOlho.EventBus.RabbitMQ
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddEventSourcing<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
        {
            services.AddScoped<IEventSourcingService>(serviceProvider => 
            {
                return new EventSourcingService(
                    serviceProvider.GetService<TDbContext>().Database.GetDbConnection());
            });

            services.AddHostedService<EventSourcingBackgroundService<TDbContext>>();

            return services;
        }
    }
}