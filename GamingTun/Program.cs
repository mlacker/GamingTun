using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GamingTun
{
    class Program
    {
        static void Main(string[] args)
        {
            var program = new Program();

            Task.Run(program.Test).Wait();
        }

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private async Task Test()
        {
            var config = new Config()
            {
                Local = IPAddress.Parse("192.168.46.10"),
                RemoteNetwork = IPAddress.Parse("192.168.46.0"),
                RemoteNetmask = IPAddress.Parse("255.255.255.0")
            };
            var remoteEP = new IPEndPoint(IPAddress.Parse("122.224.94.220"), 31194);

            using (var adapter = new TapAdapter(config))
            using (var tunnel = new Tunnel(6000, adapter))
            {
                adapter.Start();

                await tunnel.Connect(remoteEP);

                _ = tunnel.Send(cancellationTokenSource.Token);
                _ = tunnel.Receive(cancellationTokenSource.Token);

                Wait();
            }
        }

        private void Wait()
        {
            Console.WriteLine("Program is running");

            var input = string.Empty;
            while (input != "exit")
            {
                input = Console.ReadLine();
            }

            Console.WriteLine("Shutting down...");
            cancellationTokenSource.Cancel();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey(true);
        }
    }
}
