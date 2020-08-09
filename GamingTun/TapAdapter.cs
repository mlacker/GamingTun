using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Threading.Tasks;

namespace GamingTun
{
    public class TapAdapter
    {
        private readonly TunTapDevice device;
        private readonly Stream stream;
        private readonly Config config;

        public TapAdapter(Config config)
        {
            device = new TunTapDevice(config.AdapterName);
            stream = device.Stream;
            this.config = config;
        }

        public void Start()
        {
            SetIpProps();

            SetPtp();

            SetConnected();
        }

        private void SetIpProps()
        {
            // netsh interface ip set address my-tap static 10.3.0.1 255.255.255.0
        }

        private void SetPtp()
        {
            //device.ConfigTun(config.Local, config.Local & config.RemoteNetmask, config.RemoteNetmask);
            device.ConfigTun(config.Local, config.RemoteNetwork, config.RemoteNetmask);
        }

        private void SetConnected()
        {
            device.SetMediaStatus(true);
        }

        private void SetStaticIpAddress()
        {
            // netsh interface ip set address my-tap static 10.3.0.1 255.255.255.0
            var searcher = new ManagementObjectSearcher("root\\CIMV2",
                $"SELECT * FROM Win32_NetworkAdapterConfiguration Where SettingID='{device.Guid:B}'");
            var managerment = searcher.Get().OfType<ManagementObject>().First();

            if (!(bool)managerment["IPEnabled"])
                return;

            var parameters = managerment.GetMethodParameters("EnableStatic");
            parameters["IPAddress"] = new[] { config.Local.ToString() };
            parameters["SubnetMask"] = new[] { config.RemoteNetmask.ToString() };
            managerment.InvokeMethod("EnableStatic", parameters, null);

            var dnsParameters = managerment.GetMethodParameters("SetDNSServerSearchOrder");
            dnsParameters["DNSServerSearchOrder"] = new[] { "192.168.46.254" };
            managerment.InvokeMethod("SetDNSServerSearchOrder", dnsParameters, null);
        }

        public void Stop()
        {
            stream.Close();
        }

        private const int BUFFER_SIZE = 1024 * 10;

        public TapAdapter OtherAdapter { get; set; }

        public async Task Run()
        {
            byte[] buffer = new byte[BUFFER_SIZE];
            int count = 0;

            while (true)
            {
                count = await stream.ReadAsync(buffer, 0, BUFFER_SIZE);
                await OtherAdapter.stream.WriteAsync(buffer, 0, count);
                OtherAdapter.stream.Flush();
            }
        }
    }

    public class Config
    {
        public IPAddress Local { get; set; }
        public IPAddress RemoteNetwork { get; private set; }
        public IPAddress RemoteNetmask { get; private set; }
        public string AdapterName { get; set; }

        public Config()
        {
            Local = IPAddress.Parse("192.168.46.10");
            RemoteNetwork = IPAddress.Parse("192.168.46.0");
            RemoteNetmask = IPAddress.Parse("255.255.255.0");
        }
    }
}
