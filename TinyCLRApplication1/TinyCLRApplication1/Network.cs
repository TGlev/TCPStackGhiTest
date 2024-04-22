using System.Diagnostics;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Network;
using System.Net;
using System.Threading;
using GHIElectronics.TinyCLR.Pins;

namespace TinyCLRApplication1
{
    public static class Network
    {
        //This boolean can be used to determine if the device has network link
        public static bool HasLink { get; set; }
        //This boolean can be used to determine if the device has network initialized
        public static bool NetworkInitialized { get; set; }
        
        //This byte array contains the IP that the device has received
        public static byte[] DeviceIp { get; set; }

        //A reference to the gpio pin used to reset the ethernet PHY
        private static GpioPin _ethResetPin;
        
        //Some variables used to store the network settings
        private static string _ip;
        private static string _subnetMask;
        private static string _gateway;
        private static string _dns;
        private static byte[] _mac;
        
        //This function initializes the network with the given settings
        public static void Initialize(string ip, string subnetMask, string gateway, string dns, byte[] mac, int ethReset = SC20100.GpioPin.PA6)
        {
            _ethResetPin = GpioController.GetDefault().OpenPin(ethReset);
            _ethResetPin.SetDriveMode(GpioPinDriveMode.Output);

            _ethResetPin.Write(GpioPinValue.Low);
            Thread.Sleep(100);

            _ethResetPin.Write(GpioPinValue.High);
            Thread.Sleep(100);

            var networkController = NetworkController.FromName(SC20100.NetworkController.EthernetEmac);

            var networkInterfaceSetting = new EthernetNetworkInterfaceSettings();

            var networkCommunicationInterfaceSettings = new BuiltInNetworkCommunicationInterfaceSettings();

            networkInterfaceSetting.Address = new IPAddress(StringToIp(ip));
            networkInterfaceSetting.SubnetMask = new IPAddress(StringToIp(subnetMask));
            networkInterfaceSetting.GatewayAddress = new IPAddress(StringToIp(gateway));
            networkInterfaceSetting.DnsAddresses = new[] { new IPAddress(StringToIp(dns)) };

            networkInterfaceSetting.MacAddress = mac;
            networkInterfaceSetting.DhcpEnable = false;
            networkInterfaceSetting.DynamicDnsEnable = false;

            networkController.SetInterfaceSettings(networkInterfaceSetting);
            networkController.SetCommunicationInterfaceSettings(networkCommunicationInterfaceSettings);

            networkController.SetAsDefaultController();
            
            networkController.NetworkLinkConnectedChanged += NetworkController_NetworkAddressChanged;

            networkController.Enable();

            NetworkInitialized = true;

            Debug.WriteLine("Network ready");
        }

        //A function to reset the network connection
        public static void Reset()
        {
            var networkController = NetworkController.FromName(SC20100.NetworkController.EthernetEmac);
            networkController.Disable();

            Initialize(_ip, _subnetMask, _gateway, _dns, _mac);
        }

        //This event is triggered when the network link changes
        private static void NetworkController_NetworkAddressChanged(NetworkController sender, NetworkLinkConnectedChangedEventArgs e)
        {
            HasLink = e.Connected;

            var ipProperties = sender.GetIPProperties();
            var address = ipProperties.Address.GetAddressBytes();
            DeviceIp = address;

            if(HasLink)
                Debug.WriteLine($"Network link achieved, IP: {address[0]}.{address[1]}.{address[2]}.{address[3]}");
            else
                Debug.WriteLine("Network link lost");
        }

        //This function is used to convert a string ip into a byte array of length 4
        public static byte[] StringToIp(string stringIp)
        {
            var ip = new byte[4];
            var splitIp = stringIp.Split('.');

            for (var i = 0; i < 4; i++)
                ip[i] = byte.Parse(splitIp[i]);

            return ip;
        }
    }
}