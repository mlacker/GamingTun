using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GamingTun
{
    public class Controller
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly Tunnel tunnel;
        private readonly TapAdapter adapter;
        private readonly Config config;
        private readonly ILogger logger;

        public Controller(Tunnel tunnel, TapAdapter adapter, Config config, ILogger<Controller> logger)
        {
            this.tunnel = tunnel;
            this.adapter = adapter;
            this.config = config;
            this.logger = logger;

            this.cancellationTokenSource = new CancellationTokenSource();
        }

        public void Run()
        {
            try
            {
                adapter.Start();

                var conn = tunnel.Run(cancellationTokenSource.Token);

                var exit = WaitToExit();

                Task.WaitAny(conn, exit);
            }
            catch (OperationCanceledException ex)
            {
                logger.LogWarning(ex, ex.Message);
            }
            finally
            {
                Dispose();
            }

            Console.ReadKey(true);
        }

        private Task WaitToExit()
        {
            Console.WriteLine("Press enter key to exit");
            Console.ReadLine();

            logger.LogInformation("Shutting down...");
            cancellationTokenSource.Cancel();

            return Task.Delay(1000);
        }

        private void Dispose()
        {
            tunnel.Dispose();
            adapter.Dispose();
        }
    }
}
