using Confluent.Kafka;
using EventsKafkaTest.Services.Model;
using Microsoft.Extensions.Options;
using System.Linq;

namespace EventsKafkaTest.Services
{
    public interface IKafkaProducerService
    {
        Task<DeliveryResult<Null, string>> SendMessageAsync(string topic, string message);
    }

    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly KafkaProducerSettings _kafkaProduceSettings;
        private readonly ILogger<KafkaProducerService> _logger;
        private readonly IProducer<Null, string> _producer;

        public KafkaProducerService(IOptions<KafkaProducerSettings> kafkaSettings, ILogger<KafkaProducerService> logger)
        {
            _kafkaProduceSettings = kafkaSettings.Value;
            _logger = logger;
            var config = new ProducerConfig
            {
                BootstrapServers = _kafkaProduceSettings.BootstrapServers,
                SecurityProtocol = SecurityProtocol.Plaintext,
            };
            _logger.LogInformation($"Producer Service initialized: " +
               $"\n BootstrapServers = {config.BootstrapServers}");

            _producer = new ProducerBuilder<Null, string>(config).Build();
            _logger.LogInformation($"Producer built.");
        }

        public async Task<DeliveryResult<Null, string>> SendMessageAsync(string topic, string message)
        {
            DeliveryResult<Null, string>? result = null;
            try
            {
                // Send message to the specified Kafka topic
                result = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
                _logger.LogInformation($"Message '{message}' sent to topic '{topic}'.");
            }
            catch (Exception ex)
            {
                // Log any errors encountered while sending message
                _logger.LogInformation($"Error sending message to Kafka: {ex.Message}");
                throw;
            }

            return result;
        }
    }
}
