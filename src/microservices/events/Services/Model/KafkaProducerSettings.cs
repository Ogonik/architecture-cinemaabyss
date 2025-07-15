namespace EventsKafkaTest.Services.Model
{
    public class KafkaProducerSettings
    {
        public required string BootstrapServers { get; set; }    

        public required string MoviesTopicName {get;set;}

        public required string UserTopicName { get; set; }

        public required string PaymentTopicName { get; set; }

    }
}