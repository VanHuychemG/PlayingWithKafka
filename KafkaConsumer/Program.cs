namespace KafkaConsumer
{
    using Confluent.Kafka;
    using Confluent.Kafka.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class Program
    {
        private static void Main()
        {
            var config = new Dictionary<string, object>
            {
                {"group.id", "sample-consumer"},
                {"bootstrap.servers", "localhost:9092"},
                {"enable.auto.commit", "false"}
            };

            Console.WriteLine("Consumer ready...");            

            using (var consumer = new Consumer<Null, string>(config, null, new StringDeserializer(Encoding.UTF8)))
            {
                consumer.Subscribe(new[] { "hello-topic" });

                consumer.OnMessage += (_, msg) =>
                {
                    Console.WriteLine($"Topic: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} {msg.Value}");

                    consumer.CommitAsync(msg);
                };                

                while (true)
                {
                    consumer.Poll(100);
                }
            }
        }
    }
}
