using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GamingTun
{
    public class Tunnel : IDisposable
    {
        private readonly TapAdapter adapter;
        private readonly UdpClient client;

        public Tunnel(int port)
        {
            client = new UdpClient(port, AddressFamily.InterNetwork);
        }

        private volatile bool state;

        public Task Connect(IPEndPoint endPoint)
        {
            client.Connect(endPoint);

            state = true;

            return Task.CompletedTask;
        }

        public async Task Receive(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var receiveResult = await client.ReceiveAsync();

                if (!state)
                {
                    //await Connect(receiveResult.RemoteEndPoint);
                }

                var buffer = receiveResult.Buffer;

                Console.WriteLine($"From {receiveResult.RemoteEndPoint.Port}: {Encoding.UTF8.GetString(buffer)}");
            }
        }

        public async Task Send(byte[] buffer)
        {
            await client.SendAsync(buffer, buffer.Length);
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
