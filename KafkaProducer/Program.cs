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

namespace KafkaProducer
{

    public class Program
    {
        private static void Main()
        {
            var schemaRegistryConfig = new Dictionary<string, object>
             {
                { "schema.registry.url", "localhost:8081" },
                { "schema.registry.connection.timeout.ms", 5000 }, // optional
                { "schema.registry.max.cached.schemas", 10 } // optional
             };

            var producerConfig = new Dictionary<string, object>
            {
                { "schema.registry.url", "localhost:8081" },
                { "bootstrap.servers", "localhost:9092" }
            };

            Console.WriteLine("Producer ready...");

            using (var schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig))
            {
                var s = (RecordSchema)Avro.Schema.Parse(File.ReadAllText(@"C:\PROJECTDATA\PLAYGROUND\PlayingWithKafka\Kafka\Organisation.asvc"));
                schemaRegistryClient.RegisterSchemaAsync("organisation-topic-schema", s.ToString());

                using (var producer = new Producer<string, Organisation>(producerConfig, new AvroSerializer<string>(), new AvroSerializer<Organisation>()))
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

                        var deliveryReport = producer.ProduceAsync("organisation-topic", organisation.id, organisation);

                        var result = deliveryReport.Result; // synchronously waits for message to be produced.

                        Console.WriteLine($"Partition: {result.Partition}, Offset: {result.Offset}");

                        Thread.Sleep(500);
                    }
                }
            }
        }
    }
}
