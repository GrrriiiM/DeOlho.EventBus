namespace DeOlho.EventBus
{
    public class EventBusConfiguration
    {
        public string RetrySuffix { get; set; } = "-retry";
        public string FailSuffix { get; set; } = "-fail";
    }
}