using System.Reflection;

namespace DeOlho.EventBus.RabbitMQ
{
    public class EventBusRabbitMQConfiguration
    {
        
        public string HostName { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; } = "/";
        public string ExchangeName { get; set; } = "DeOlho";
        public string QueueName { get; set; } = Assembly.GetEntryAssembly().GetName().Name;
    }
}