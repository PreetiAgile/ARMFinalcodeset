namespace ARMCommon.Helpers
{
    public interface IRabbitMQProducer
    {
        abstract bool SendMessage<T>(T message, string queueName);

       abstract bool SendMessages<T>(T message, string queueName, bool trace = false , int delayTimeInMs =0 );
       abstract bool DeleteAllMessagesFromQueue(string queueName);
    }
}
