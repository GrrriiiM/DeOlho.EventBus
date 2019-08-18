using System;
using System.Reflection;
using System.Text;
using DeOlho.EventBus.Message;
using Newtonsoft.Json;

namespace DeOlho.EventBus.RabbitMQ.EventSourcing
{
    public class EventSourcing
    {
        public EventSourcing(EventBusMessage message)
        {
            TypeName = message.GetType().Name;
            var json = JsonConvert.SerializeObject(message);
            Content = Encoding.UTF8.GetBytes(json);
            DateTimeCreation = DateTime.Now;
        }

        public string AssemblyName { get; set; }
        public string TypeName { get; set; }
        public DateTime DateTimeCreation { get; set; }
        public byte[] Content { get; set; }
        public int Status { get; set; }
    }
}