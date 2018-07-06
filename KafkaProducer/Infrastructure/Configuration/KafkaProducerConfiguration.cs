using Kafka.Infrastructure.Configuration;

namespace KafkaProducer.Infrastructure.Configuration
{
    public class KafkaProducerConfiguration: KafkaConfiguration
    {
        public static string Section = "Kafka";

        public string AvroSchema { get; set; }
    }
}
