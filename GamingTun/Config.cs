using Microsoft.Extensions.Options;
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

        public Config(IOptionsSnapshot<ConfigOptions> optionsSnapshot)
        {
            var options = optionsSnapshot.Value;

            Local = options.Parse(options.Local);
            if (!string.IsNullOrEmpty(options.Remote))
                Remote = options.Parse(options.Remote);

            DriverName = options.DriverName;
            Address = IPAddress.Parse(options.Address);
            Netmask = IPAddress.Parse(options.Netmask);

            var networkBytes = Address.GetAddressBytes();
            for (int i = 0; i < networkBytes.Length; i++)
            {
                networkBytes[i] &= Netmask.GetAddressBytes()[i];
            }
            Network = new IPAddress(networkBytes);
        }
    }

    public class ConfigOptions
    {
        public ConfigOptions()
        {
            DriverName = string.Empty;
            Address = "192.168.46.10";
            Netmask = "255.255.255.0";
            Local = "0.0.0.0:6000";
        }

        public string DriverName { get; set; }
        public string Address { get; set; }
        public string Netmask { get; set; }
        public string Local { get; set; }
        public string Remote { get; set; }

        public IPEndPoint Parse(string parts) =>
            new IPEndPoint(
                IPAddress.Parse(parts.Split(':')[0]),
                int.Parse(parts.Split(':')[1])
            );
    }
}
