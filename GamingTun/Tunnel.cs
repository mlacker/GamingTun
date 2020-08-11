using Microsoft.Extensions.Logging;
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
        private const int BUFFER_SIZE = 1460;

        private readonly UdpClient client;
        private readonly Stream stream;
        private readonly Config config;
        private readonly ILogger<Tunnel> logger;

        private IPEndPoint remoteEP;
        private CancellationToken cancellationToken;

        public Tunnel(TapAdapter adapter, Config config, ILogger<Tunnel> logger)
        {
            client = new UdpClient(config.Local);
            stream = adapter.Stream;
            this.config = config;
            this.logger = logger;

            logger.LogInformation($"Network : {config.Address} {config.Netmask} {config.DriverName}");
            logger.LogInformation($"Listen  : {config.Local.Address}:{config.Local.Port}");
            if (config.Remote != null)
                logger.LogInformation($"Connect : {config.Remote.Address}:{config.Remote.Port}");
        }

        public Task Run(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;

            if (config.Remote != null)
                Connect(config.Remote);

            var send = new Task(Send, cancellationToken, TaskCreationOptions.LongRunning);
            var recv = new Task(Receive, cancellationToken, TaskCreationOptions.LongRunning);

            send.Start();
            recv.Start();

            return Task.WhenAll(send, recv);
        }

        public void Connect(IPEndPoint endPoint)
        {
            remoteEP = endPoint;
        }

        public void Receive()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var buffer = client.Receive(ref remoteEP);

                    LogBuffer("RecvForm", buffer, buffer.Length);

                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();
                }
                catch (SocketException ex)
                {
                    logger.LogWarning(ex, "Recv");
                }
            }
        }

        public void Send()
        {
            var buffer = new byte[BUFFER_SIZE];

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var count = stream.Read(buffer, 0, BUFFER_SIZE);

                    LogBuffer("SendTo  ", buffer, count);

                    client.Send(buffer, count, remoteEP);
                }
                catch (OperationCanceledException ex)
                {
                    logger.LogWarning(ex, "Send");
                }
            }
        }

        public void Dispose()
        {
            client.Dispose();
        }

        private void LogBuffer(string action, byte[] buffer, int count)
        {
            if (!logger.IsEnabled(LogLevel.Trace))
            {
                return;
            }

            StringBuilder message = new StringBuilder();

            var protocolType = (ProtocolType)buffer[9];

            message.AppendFormat("{0} {1}:{2} Protocol:{3} ", action, remoteEP.Address, remoteEP.Port, protocolType);

            if (protocolType == ProtocolType.Icmp)
            {
                var pingType = string.Empty;
                switch (buffer[20])
                {
                    case 0x00:
                        pingType = "Reply ";
                        break;
                    case 0x08:
                        pingType = "Request ";
                        break;
                    default:
                        break;
                }
                message.Append(pingType);
            }

            message.AppendLine();
            message.AppendLine(BitConverter.ToString(buffer, 20, count - 20).Replace('-', ' '));

            logger.LogTrace(message.ToString());
        }
    }
}
