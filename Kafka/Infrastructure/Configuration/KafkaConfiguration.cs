using System.Collections.Generic;

namespace Kafka.Infrastructure.Configuration
{
    public class KafkaConfiguration
    {
        public string TopicName => "organisation-topic";

        public Dictionary<string, object> SchemaRegistryConfiguration => new Dictionary<string, object>
        {
            {"schema.registry.url", "localhost:8081"},
            {"schema.registry.connection.timeout.ms", 5000}, // optional
            {"schema.registry.max.cached.schemas", 10} // optional
        };

        public Dictionary<string, object> ProducerConfiguration => new Dictionary<string, object>
        {
            {"bootstrap.servers", "localhost:9092"},
            // optional avro serializer properties:
            {"avro.serializer.buffer.bytes", 50},
            {"avro.serializer.auto.register.schemas", false}
        };

        public Dictionary<string, object> ConsumerConfiguration => new Dictionary<string, object>
        {
            {"group.id", "organisation-consumer"},
            {"bootstrap.servers", "localhost:9092"},
            {"enable.auto.commit", "false"}
        };
    }
}