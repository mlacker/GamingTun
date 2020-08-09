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
        private const int BUFFER_SIZE = 1464;

        private readonly UdpClient client;
        private readonly Stream stream;
        private IPEndPoint remoteEndPoint;

        public Tunnel(int port, TapAdapter adapter)
        {
            client = new UdpClient(port, AddressFamily.InterNetwork);
            stream = adapter.Stream;
        }

        private volatile bool state;

        public Task Connect(IPEndPoint endPoint)
        {
            remoteEndPoint = endPoint;

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
                    await Connect(receiveResult.RemoteEndPoint);
                }

                var buffer = receiveResult.Buffer;

                Console.WriteLine($"From {remoteEndPoint.Address}:{remoteEndPoint.Port}: {Encoding.UTF8.GetString(buffer)}");

                await stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }
        }

        public async Task Send(CancellationToken cancellationToken)
        {
            var buffer = new byte[BUFFER_SIZE];

            while (!cancellationToken.IsCancellationRequested)
            {
                var count = await stream.ReadAsync(buffer, 0, BUFFER_SIZE, cancellationToken);
                
                _ = client.SendAsync(buffer, count, remoteEndPoint);
            }
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
