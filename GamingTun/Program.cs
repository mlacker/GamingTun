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

            program.Setup();

            Task.Run(program.Tunnel).Wait();

            program.Wait();
        }

        private void Adapter()
        {
            var config1 = new Config()
            {
                AdapterName = "Tun1",
                Local = IPAddress.Parse("192.168.46.10")
            };
            var adapter1 = new TapAdapter(config1);
            var config2 = new Config()
            {
                AdapterName = "Tun2",
                Local = IPAddress.Parse("192.168.46.20")
            };
            var adapter2 = new TapAdapter(config2);
            adapter1.Start();
            adapter2.Start();
            adapter1.OtherAdapter = adapter2;
            adapter2.OtherAdapter = adapter1;
            Task.Run(adapter1.Run);
            Task.Run(adapter2.Run);
            Console.WriteLine("Program is running");
            adapter1.Stop();
            adapter2.Stop();
        }

        private async Task Tunnel()
        {
            using (Tunnel tun1 = new Tunnel(6000))
            using (Tunnel tun2 = new Tunnel(6050))
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var task1 = tun1.Receive(cancellationTokenSource.Token);
                var task2 = tun2.Receive(cancellationTokenSource.Token);

                await tun1.Connect(new IPEndPoint(IPAddress.Parse("172.16.10.38"), 6050));

                string input;

                do
                {
                    input = Console.ReadLine();

                    await tun1.Send(Encoding.Default.GetBytes(input));

                } while (input != "exit");

                cancellationTokenSource.Cancel();
            }
        }

        private void Setup()
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
        }

        private void Wait()
        {
            Console.WriteLine("Press any key to exit");
            Console.ReadKey(true);
            Console.WriteLine("Shutting down...");
        }

        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
