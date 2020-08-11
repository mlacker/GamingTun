using System;
using System.IO;

namespace GamingTun
{
    public class TapAdapter : IDisposable
    {
        private readonly TunTapDevice device;
        private readonly Stream stream;
        private readonly Config config;

        public TapAdapter(Config config)
        {
            this.config = config;

            device = new TunTapDevice(config.DriverName);
            stream = device.Stream;
        }

        public Stream Stream => stream;

        public void Start()
        {
            SetIpProps();

            SetPtp();

            SetConnected();
        }

        private void SetIpProps()
        {
            // netsh interface ip set address my-tap static 10.3.0.1 255.255.255.0

            //var searcher = new ManagementObjectSearcher("root\\CIMV2",
            //    $"SELECT * FROM Win32_NetworkAdapterConfiguration Where SettingID='{device.Guid:B}'");
            //var managerment = searcher.Get().OfType<ManagementObject>().First();

            //if (!(bool)managerment["IPEnabled"])
            //    return;

            //var parameters = managerment.GetMethodParameters("EnableStatic");
            //parameters["IPAddress"] = new[] { config.Local.ToString() };
            //parameters["SubnetMask"] = new[] { config.RemoteNetmask.ToString() };
            //managerment.InvokeMethod("EnableStatic", parameters, null);

            //var dnsParameters = managerment.GetMethodParameters("SetDNSServerSearchOrder");
            //dnsParameters["DNSServerSearchOrder"] = new[] { "192.168.46.254" };
            //managerment.InvokeMethod("SetDNSServerSearchOrder", dnsParameters, null);

            //netsh interface ipv4 set interface Tun2 mtu=1460
        }

        private void SetPtp()
        {
            device.ConfigTun(config.Address, config.Network, config.Netmask);
        }

        private void SetConnected()
        {
            device.SetMediaStatus(true);
        }

        public void Dispose()
        {
            stream.Dispose();
        }
    }
}
