using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;

namespace GamingTun
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("config.json", optional: true, reloadOnChange: true)
                .AddCommandLine(args)
                .Build();

            var serviceProvider = ConfigureServices(new ServiceCollection(), configuration);
            var controller = serviceProvider.GetRequiredService<Controller>();

            controller.Run().Wait();
        }


        private static ServiceProvider ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddLogging(builder => builder
                    .AddConfiguration(configuration.GetSection("Logging"))
                    .AddNLog()
                )
                .AddSingleton<Controller>()
                .AddTransient<Config>();

            var profile = configuration.GetValue<string>("profile");
            if (string.IsNullOrEmpty(profile))
            {
                Console.Write("Select Profile[default]: ");
                profile = Console.ReadLine();
            }

            switch (profile)
            {
                default:
                case "1":
                    services.
                        Configure<ConfigOptions>(configuration.GetSection("Profile1"));
                    break;
                case "2":
                    services.
                        Configure<ConfigOptions>(configuration.GetSection("Profile2"));
                    break;
                case "3":
                    services.
                        Configure<ConfigOptions>(configuration.GetSection("Profile3"));
                    break;
            }

            return services.BuildServiceProvider();
        }
    }
}
