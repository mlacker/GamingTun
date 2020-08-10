using System.Net;

namespace GamingTun
{
    public class Config
    {
        public IPAddress Address { get; set; }
        public IPAddress Network { get; set; }
        public IPAddress Netmask { get; set; }
        public IPEndPoint Local { get; set; }
        public IPEndPoint Remote { get; set; }
        public string DriverName { get; set; }

        public Config(string address, string netmask, int localPort, string driverName = "")
        {
            Address = IPAddress.Parse(address);
            Netmask = IPAddress.Parse(netmask);
            DriverName = driverName;

            Local = new IPEndPoint(IPAddress.Any, localPort);

            var networkBytes = Address.GetAddressBytes();
            for (int i = 0; i < networkBytes.Length; i++)
            {
                networkBytes[i] &= Netmask.GetAddressBytes()[i];
            }
            Network = new IPAddress(networkBytes);
        }

        public Config(string local, string netmask, int localPort, string remote, int remotePort, string driverName = "")
            : this(local, netmask, localPort, driverName)
        {
            Remote = new IPEndPoint(IPAddress.Parse(remote), remotePort);
        }
    }
}
