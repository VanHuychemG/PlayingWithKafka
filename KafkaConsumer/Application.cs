using KafkaConsumer.Services;
using Microsoft.Extensions.Logging;

namespace KafkaConsumer
{
    public class Application
    {
        private readonly IKafkaConsumerService _kafkaConsumerService;
        private readonly ILogger<Application> _logger;

        public Application(
            IKafkaConsumerService kafkaConsumerService,
            ILogger<Application> logger)
        {
            _kafkaConsumerService = kafkaConsumerService;
            _logger = logger;
        }

        public void Run()
        {
            _logger.LogInformation("Application started.");

            _kafkaConsumerService.Consume();

            _logger.LogInformation("Application ended.");
        }
    }
}
