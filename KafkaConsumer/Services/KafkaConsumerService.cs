using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Confluent.SchemaRegistry;
using Kafka;
using KafkaConsumer.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace KafkaConsumer.Services
{
    public class KafkaConsumerService : IKafkaConsumerService
    {
        private readonly KafkaConsumerConfiguration _consumerConfiguration;
        private readonly ILogger<KafkaConsumerService> _logger;

        public KafkaConsumerService(
            IOptions<KafkaConsumerConfiguration> options,
            ILogger<KafkaConsumerService> logger)
        {
            _consumerConfiguration = options.Value;
            _logger = logger;
        }

        public void Consume()
        {
            _logger.LogInformation("Consumer ready...");

            using (var schemaRegistryClient = new CachedSchemaRegistryClient(_consumerConfiguration.SchemaRegistryConfiguration))
            {
                using (var consumer = new Consumer<string, Organisation>(_consumerConfiguration.ConsumerConfiguration, new AvroDeserializer<string>(schemaRegistryClient), new AvroDeserializer<Organisation>(schemaRegistryClient)))
                {
                    consumer.OnMessage += (o, e)
                        => Console.WriteLine($"organisation key name: {e.Key}, organisation value name: {e.Value.name}");

                    consumer.OnError += (_, e)
                        => Console.WriteLine("Error: " + e.Reason);

                    consumer.OnConsumeError += (_, e)
                        => Console.WriteLine("Consume error: " + e.Error.Reason);

                    consumer.Subscribe(_consumerConfiguration.TopicName);

                    _logger.LogInformation($"Subscribed to: [{string.Join(", ", consumer.Subscription)}]");

                    var cancelled = false;

                    Console.CancelKeyPress += (_, e) =>
                    {
                        e.Cancel = true; // prevent the process from terminating.
                        cancelled = true;
                    };

                    while (!cancelled)
                    {
                        if (!consumer.Consume(out var msg, TimeSpan.FromSeconds(1)))
                            continue;

                        _logger.LogInformation($"Topic: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} {msg.Value.name}");

                        if (msg.Offset % 5 != 0) continue;

                        _logger.LogInformation($"Committing offset");
                        var committedOffsets = consumer.CommitAsync(msg).Result;
                        _logger.LogInformation($"Committed offset: {committedOffsets}");
                    }
                }
            }
        }
    }
}