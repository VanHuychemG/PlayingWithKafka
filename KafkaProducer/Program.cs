using AutoFixture;
using Avro;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Confluent.SchemaRegistry;
using Kafka;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Xunit;

namespace KafkaProducer
{

    public class Program
    {
        private static void Main()
        {
            var topicName = "organisation-topic";

            var schemaRegistryConfig = new Dictionary<string, object>
             {
                { "schema.registry.url", "localhost:8081" },
                { "schema.registry.connection.timeout.ms", 5000 }, // optional
                { "schema.registry.max.cached.schemas", 10 } // optional
             };

            var producerConfig = new Dictionary<string, object>
            {
                { "bootstrap.servers", "localhost:9092" },
                // optional avro serializer properties:
                { "avro.serializer.buffer.bytes", 50 },
                { "avro.serializer.auto.register.schemas", false }
            };

            Console.WriteLine("Producer ready...");

            using (var schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig))
            {
                var avroSchema = (RecordSchema)Avro.Schema.Parse(File.ReadAllText(@"C:\PROJECTDATA\PLAYGROUND\PlayingWithKafka\Kafka\Organisation.asvc"));

                var keySubjectName = schemaRegistryClient.ConstructKeySubjectName(topicName);
                Assert.Equal(topicName + "-key", keySubjectName);
                var key = schemaRegistryClient.RegisterSchemaAsync(keySubjectName, "{ \"type\": \"string\" }").Result;
                Assert.Contains(keySubjectName, schemaRegistryClient.GetAllSubjectsAsync().Result);

                var valueSubjectName = schemaRegistryClient.ConstructValueSubjectName(topicName);
                Assert.Equal(topicName + "-value", valueSubjectName);
                var value = schemaRegistryClient.RegisterSchemaAsync(valueSubjectName, avroSchema.ToString()).Result;
                Assert.Contains(valueSubjectName, schemaRegistryClient.GetAllSubjectsAsync().Result);

                var schema = schemaRegistryClient.GetSchemaAsync(value).Result;
                Assert.Equal(avroSchema.ToString(), schema);

                using (var producer = new Producer<string, Organisation>(producerConfig, new AvroSerializer<string>(schemaRegistryClient), new AvroSerializer<Organisation>(schemaRegistryClient)))
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

                        var deliveryReport = producer.ProduceAsync(topicName, organisation.id, organisation);

                        var result = deliveryReport.Result; // synchronously waits for message to be produced.

                        Console.WriteLine($"Partition: {result.Partition}, Offset: {result.Offset}");

                        Thread.Sleep(500);
                    }
                }
            }
        }
    }
}
