using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeOlho.EventBus.Abstractions;
using DeOlho.EventBus.Message;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace DeOlho.EventBus.RabbitMQ.EventSourcing
{
    public class EventSourcingBackgroundService<TDbContext> : BackgroundService where TDbContext : DbContext
    {
        readonly EventSourcingDbContext _eventSourcingDbContext;
        readonly IEventBus _eventBus;

        List<Assembly> assemblies = new List<Assembly>();
        List<Type> types = new List<Type>();

        public EventSourcingBackgroundService(
            IServiceProvider serviceProvider,
            IEventBus eventBus)
        {
            using(var scope = serviceProvider.CreateScope())
            {
                _eventSourcingDbContext = new EventSourcingDbContext(
                    new DbContextOptionsBuilder<EventSourcingDbContext>()
                        .UseMySql(scope.ServiceProvider.GetService<TDbContext>().Database.GetDbConnection().ConnectionString)
                        .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                        .Options);
                    _eventBus = eventBus;
            }
        }
        
        public async override Task StartAsync(CancellationToken cancellationToken)
        {
            await _eventSourcingDbContext.Database.MigrateAsync(cancellationToken);
            await base.StartAsync(cancellationToken);
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                foreach(var eventLog in _eventSourcingDbContext.EventLogs.Where(_ => _.Status == 0).ToList())
                {
                    var assembly = assemblies.FirstOrDefault(_ => _.FullName == eventLog.AssemblyName);
                    if (assembly == null)
                    {
                        assembly = Assembly.Load(eventLog.AssemblyName);
                        assemblies.Add(assembly);
                    }
                    var type = types.FirstOrDefault(_ => _.FullName == eventLog.TypeName);
                    if (type == null)
                    {
                        type = assembly.GetType(eventLog.TypeName);
                        types.Add(type);
                    }

                    var json = Encoding.UTF8.GetString(eventLog.Content);
                    var obj = JsonConvert.DeserializeObject(json, type);

                    _eventBus.Publish(type, (EventBusMessage)obj);

                    _eventSourcingDbContext.Remove(eventLog);

                    await _eventSourcingDbContext.SaveChangesAsync(stoppingToken);
                }
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}