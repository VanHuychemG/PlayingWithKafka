using KafkaProducer.Services;
using Microsoft.Extensions.Logging;

namespace KafkaProducer
{
    public class Application
    {
        private readonly IKafkaProducerService _kafkaProducerService;
        private readonly ILogger<Application> _logger;

        public Application(
            IKafkaProducerService kafkaProducerService,
            ILogger<Application> logger)
        {
            _kafkaProducerService = kafkaProducerService;
            _logger = logger;
        }

        public void Run()
        {
            _logger.LogInformation("Application started.");

            _kafkaProducerService.Produce();

            _logger.LogInformation("Application ended.");
        }
    }
}