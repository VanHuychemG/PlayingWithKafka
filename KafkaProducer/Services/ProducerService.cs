using AutoFixture;
using Avro;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Confluent.SchemaRegistry;
using Kafka;
using KafkaProducer.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading;
using Xunit;

namespace KafkaProducer.Services
{
    public interface IKafkaProducerService
    {
        void Produce();
    }

    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly KafkaProducerConfiguration _producerConfiguration;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(
            IOptions<KafkaProducerConfiguration> options,
            ILogger<KafkaProducerService> logger)
        {
            _producerConfiguration = options.Value;
            _logger = logger;

            PrepSchema();
        }

        private void PrepSchema()
        {
            _logger.LogInformation("Prepping schema...");

            using (var schemaRegistryClient = new CachedSchemaRegistryClient(_producerConfiguration.SchemaRegistryConfiguration))
            {
                var avroSchema = (RecordSchema)Avro.Schema.Parse(File.ReadAllText(_producerConfiguration.AvroSchema));

                var keySubjectName = schemaRegistryClient.ConstructKeySubjectName(_producerConfiguration.TopicName);
                Assert.Equal(_producerConfiguration.TopicName + "-key", keySubjectName);
                var key = schemaRegistryClient.RegisterSchemaAsync(keySubjectName, "{ \"type\": \"string\" }").Result;
                Assert.Contains(keySubjectName, schemaRegistryClient.GetAllSubjectsAsync().Result);

                var valueSubjectName = schemaRegistryClient.ConstructValueSubjectName(_producerConfiguration.TopicName);
                Assert.Equal(_producerConfiguration.TopicName + "-value", valueSubjectName);
                var value = schemaRegistryClient.RegisterSchemaAsync(valueSubjectName, avroSchema.ToString()).Result;
                Assert.Contains(valueSubjectName, schemaRegistryClient.GetAllSubjectsAsync().Result);

                var schema = schemaRegistryClient.GetSchemaAsync(value).Result;
                Assert.Equal(avroSchema.ToString(), schema);
            }
        }

        public void Produce()
        {
            _logger.LogInformation("Producer ready...");

            using (var schemaRegistryClient = new CachedSchemaRegistryClient(_producerConfiguration.SchemaRegistryConfiguration))
            {
                using (var producer = new Producer<string, Organisation>(_producerConfiguration.GlobalConfiguration, new AvroSerializer<string>(schemaRegistryClient), new AvroSerializer<Organisation>(schemaRegistryClient)))
                {
                    var cancelled = false;

                    Console.CancelKeyPress += (_, e) =>
                    {
                        e.Cancel = true; // prevent the process from terminating.
                        cancelled = true;
                    };

                    while (!cancelled)
                    {
                        var fixture = new Fixture();

                        var organisation = fixture.Build<Organisation>().Create();

                        var deliveryReport = producer.ProduceAsync(_producerConfiguration.TopicName, organisation.id, organisation);

                        var result = deliveryReport.Result; // synchronously waits for message to be produced.

                        _logger.LogInformation($"Partition: {result.Partition}, Offset: {result.Offset}");

                        Thread.Sleep(500);
                    }
                }
            }
        }
    }
}