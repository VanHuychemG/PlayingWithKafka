namespace KafkaProducer
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
                {"bootstrap.servers", "localhost:9092"}
            };

            Console.WriteLine("Producer ready...");
            Console.WriteLine("Type some text and hit enter...");

            using (var producer = new Producer<Null, string>(config, null, new StringSerializer(Encoding.UTF8)))
            {
                string text = null;

                while (text != "exit")
                {
                    text = Console.ReadLine();
                    producer.ProduceAsync("hello-topic", null, text);
                }

                producer.Flush(100);
            }
        }
    }
}
