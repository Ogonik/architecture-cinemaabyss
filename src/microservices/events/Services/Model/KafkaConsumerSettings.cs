namespace EventsKafkaTest.Services.Model
{
    public class KafkaConsumerSettings
    {
        public required string BootstrapServers { get; set; }
        public required string ConsumerGroup { get; set; }

        public required string MoviesTopicName { get; set; }    
        public required string UsersTopicName { get; set; } 
        public required string PaymentsTopicName { get; set; }

    }
}