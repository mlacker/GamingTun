using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GamingTun
{
    public class Controller
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Config config;

        public Controller()
        {
        }

        public void Setup()
        {
            Console.Write("Select profile[default]: ");
            var profile = Console.ReadLine();

            switch (profile)
            {
                default:
                case "1":
                    config = new Config("192.168.46.10", "255.255.255.0", 6000, "172.16.10.38", 7000, "Tun1");
                    break;
                case "2":
                    config = new Config("192.168.46.20", "255.255.255.0", 7000, "Tun2");
                    break;
            }

            Console.WriteLine($"Network : {config.Address} {config.Netmask} {config.DriverName}");
            Console.WriteLine($"Network : {config.Address} {config.Netmask} {config.DriverName}");
            Console.WriteLine($"Listen  : {config.Local.Address}:{config.Local.Port}");
            if (config.Remote != null)
                Console.WriteLine($"Connect : {config.Remote.Address}:{config.Remote.Port}");
        }

        public async Task Run()
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

        private Task WaitToExit()
        {
            Console.WriteLine("Press any key to exit");
            Console.ReadKey(true);

            Console.WriteLine("Shutting down...");
            cancellationTokenSource.Cancel();

            return Task.Delay(1000);
        }
    }
}
