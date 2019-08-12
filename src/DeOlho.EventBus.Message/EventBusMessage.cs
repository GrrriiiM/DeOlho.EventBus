using System;
using System.Reflection;

namespace DeOlho.EventBus.Message
{
    public class EventBusMessage
    {
        public EventBusMessage(string messageId)
        {
            MessageId = messageId;
            MessageDate = DateTime.Now;
            MessageOrigin = Assembly.GetEntryAssembly().GetName().Name;    
        }

        public string MessageId { get; private set; }
        public DateTime MessageDate { get; private set; }
        public string MessageOrigin { get; private set; }

    }
}