using System;
using System.Reflection;
using System.Text;
using DeOlho.EventBus.Message;
using Newtonsoft.Json;

namespace DeOlho.EventBus.EventSourcing
{
    public class EventLog
    {
        private EventLog() {}

        public EventLog(EventBusMessage message)
        {
            AssemblyName = message.GetType().Assembly.GetName().Name;
            TypeName = message.GetType().FullName;
            var json = JsonConvert.SerializeObject(message);
            Content = Encoding.UTF8.GetBytes(json);
            DateTimeCreation = DateTime.Now;
        }

        public long Id { get; private set; }

        public string AssemblyName { get; private set; }
        public string TypeName { get; private set; }
        public DateTime DateTimeCreation { get; private set; }
        public byte[] Content { get; private set; }
        public int Status { get; private set; }
    }
}