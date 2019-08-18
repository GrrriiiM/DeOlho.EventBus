using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeOlho.EventBus.Abstractions;
using DeOlho.EventBus.Message;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace DeOlho.EventBus.RabbitMQ.EventSourcing
{
    public class EventSourcingBackgroundService : BackgroundService
    {
        readonly DeOlhoDbContext _dbContext;
        readonly IEventBus _eventBus;

        List<Assembly> assemblies = new List<Assembly>();
        List<Type> types = new List<Type>();

        public EventSourcingBackgroundService()
        {
            
        }
        
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                foreach(var eventSourcing in _dbContext.EventsSourcing.Where(_ => _.Status == 0).ToList())
                {
                    var assembly = assemblies.FirstOrDefault(_ => _.FullName == eventSourcing.AssemblyName);
                    if (assembly == null)
                    {
                        assembly = Assembly.Load(eventSourcing.AssemblyName);
                        assemblies.Add(assembly);
                    }
                    var type = types.FirstOrDefault(_ => _.FullName == eventSourcing.TypeName);
                    if (type == null)
                    {
                        type = assembly.GetType(eventSourcing.AssemblyName);
                        types.Add(type);
                    }

                    var json = Encoding.UTF8.GetString(eventSourcing.Content);
                    var obj = (EventBusMessage)JsonConvert.DeserializeObject(json);

                    _eventBus.Publish(type, obj);

                    await _dbContext.SaveChangesAsync(stoppingToken);
                }
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}