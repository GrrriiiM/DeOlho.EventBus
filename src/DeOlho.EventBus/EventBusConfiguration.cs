namespace DeOlho.EventBus
{
    public class EventBusConfiguration
    {
        public string RetrySuffix { get; set; } = "-retry";
        public string FailSuffix { get; set; } = "-fail";
        public int[] ConsumerRetryInterval { get; set; } = new int[] { 1, 10, 30 };
    }
}