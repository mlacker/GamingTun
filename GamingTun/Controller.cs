using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GamingTun
{
    public class Controller
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly Config config;
        private readonly ILogger logger;

        public Controller(ILogger<Controller> logger, Config config)
        {
            this.logger = logger;
            this.config = config;

            this.cancellationTokenSource = new CancellationTokenSource();

            logger.LogInformation($"Network : {config.Address} {config.Netmask} {config.DriverName}");
            logger.LogInformation($"Listen  : {config.Local.Address}:{config.Local.Port}");
            if (config.Remote != null)
                logger.LogInformation($"Connect : {config.Remote.Address}:{config.Remote.Port}");
        }

        public async Task Run()
        {
            try
            {
                using (var adapter = new TapAdapter(config))
                using (var tunnel = new Tunnel(config.Local, adapter))
                {
                    adapter.Start();

                    if (config.Remote != null)
                        await tunnel.Connect(config.Remote);

                    var send = tunnel.Send(cancellationTokenSource.Token);
                    var recv = tunnel.Receive(cancellationTokenSource.Token);

                    var exit = WaitToExit();

                    Task.WaitAll(new[] { send, recv, exit }, cancellationTokenSource.Token);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        private Task WaitToExit()
        {
            Console.WriteLine("Press any key to exit");
            Console.ReadKey(true);

            logger.LogInformation("Shutting down...");
            cancellationTokenSource.Cancel();

            return Task.Delay(1000);
        }
    }
}
