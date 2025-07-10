using Confluent.Kafka;
using EventsKafkaTest.Services.Model;
using Microsoft.Extensions.Options;

namespace EventsKafkaTest.Services
{
    public class KafkaConsumerService : IHostedService
    {
        private readonly KafkaConsumerSettings _kafkaConsumerSettings;
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly IConsumer<Null, string> _consumer;

        public KafkaConsumerService(IOptions<KafkaConsumerSettings> kafkaSettings, ILogger<KafkaConsumerService> logger)
        {
            _kafkaConsumerSettings = kafkaSettings.Value;
            _logger = logger;

            var config = new ConsumerConfig
            {
                BootstrapServers = _kafkaConsumerSettings.BootstrapServers,
                GroupId = _kafkaConsumerSettings.ConsumerGroup,
                AutoOffsetReset = AutoOffsetReset.Latest,
                SecurityProtocol = SecurityProtocol.Plaintext,
            };
            _logger.LogInformation($"Consumer Service initialized: " +
                $"\n BootstrapServers = {config.BootstrapServers} \n GroupId = {config.GroupId} \n AutoOffsetReset = {config.AutoOffsetReset}");

            _consumer = new ConsumerBuilder<Null, string>(config).Build();

            _logger.LogInformation($"Consumers built.");

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(new string[] { _kafkaConsumerSettings.MoviesTopicName, _kafkaConsumerSettings.UsersTopicName, _kafkaConsumerSettings.PaymentsTopicName });

            _logger.LogInformation($"Start consuming from topics...");
            Task.Run(() => ConsumeMessages(cancellationToken), cancellationToken);
            return Task.CompletedTask;
        }

        private async Task ConsumeMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken);


                    if (consumeResult != null && consumeResult.Message != null)
                    {
                        _logger.LogInformation($"Topic: {consumeResult.Topic}, \n Received message: {consumeResult.Message.Value}");

                       
                        // Commit the offset (important for at-least-once delivery)
                        try
                        {
                            _consumer.Commit(consumeResult);
                        }
                        catch (KafkaException e)
                        {
                            _logger.LogError($"Commit error: {e.Error.Reason}");
                        }
                    }

                }
                catch (ConsumeException e)
                {
                    _logger.LogError($"Consume error: {e.Error.Reason}");
                }
            }
            _logger.LogInformation("Kafka consumer stopped.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _consumer.Close();
            _consumer.Dispose();

            return Task.CompletedTask;
        }
    }
}
