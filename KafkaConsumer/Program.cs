using Avro;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Confluent.SchemaRegistry;
using Kafka;
using System;
using System.Collections.Generic;
using System.IO;

namespace KafkaConsumer
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

            var consumerConfig = new Dictionary<string, object>
            {
                { "schema.registry.url", "localhost:8081" },
                {"group.id", "organisation-consumer"},
                {"bootstrap.servers", "localhost:9092"},
                {"enable.auto.commit", "false"}
            };

            Console.WriteLine("Consumer ready...");

            using (var schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig))
            {
                using (var consumer = new Consumer<string, Organisation>(consumerConfig, new AvroDeserializer<string>(), new AvroDeserializer<Organisation>()))
                {
                    consumer.OnMessage += (o, e)
                        => Console.WriteLine($"organisation key name: {e.Key}, organisation value name: {e.Value.name}");

                    consumer.OnError += (_, e)
                        => Console.WriteLine("Error: " + e.Reason);

                    consumer.OnConsumeError += (_, e)
                        => Console.WriteLine("Consume error: " + e.Error.Reason);

                    consumer.Subscribe("organisation-topic");

                    Console.WriteLine($"Subscribed to: [{string.Join(", ", consumer.Subscription)}]");

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

                        Console.WriteLine($"Topic: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} {msg.Value.name}");

                        if (msg.Offset % 5 != 0) continue;

                        Console.WriteLine($"Committing offset");
                        var committedOffsets = consumer.CommitAsync(msg).Result;
                        Console.WriteLine($"Committed offset: {committedOffsets}");
                    }
                }
            }
        }
    }
}
