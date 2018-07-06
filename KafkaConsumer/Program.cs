using KafkaConsumer.Infrastructure.Configuration;
using KafkaConsumer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;

namespace KafkaConsumer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var services = new ServiceCollection();

            ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();

            var app = serviceProvider.GetService<Application>();

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var loggerFactory = new LoggerFactory()
                .AddConsole()
                .AddDebug();

            services.AddSingleton(loggerFactory);
            services.AddLogging();

            var configuration = GetConfiguration();
            services.AddSingleton<IConfiguration>(configuration);

            services.AddOptions();
            services.Configure<KafkaConsumerConfiguration>(configuration.GetSection(KafkaConsumerConfiguration.Section));

            services.AddSingleton<IKafkaConsumerService, KafkaConsumerService>();
            services.AddTransient<Application>();
        }

        private static IConfigurationRoot GetConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true);

            return configuration.Build();
        }
    }
}