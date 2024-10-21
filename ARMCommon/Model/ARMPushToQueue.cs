namespace ARM_APIs.Model
{
    public class ARMQueueData
    {
        public string? queuedata { get; set; }
        public Dictionary<string, string>? queuejson { get; set; }
        public string queuename { get; set; }
        public string? signalrclient { get; set; }
        public int? timespandelay { get; set; }
        public bool? trace { get; set; }
    }

   
}
