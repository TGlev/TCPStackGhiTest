using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Native;

namespace TinyCLRApplication1
{
    /*
     * This code is ment to reproduce an issue with the TCP stack in TinyCLR devices.
     * When running below code with the default 10ms timeout, the TCP socket will get in a timeout loop.
     * In order to trigger this, use putty (or similar) to connect to the TCP socket. Default is 192.168.31.55:6161
     * After connecting, you should see anywhere between 1 and 3 NMEA sentences appear in your window. After this, no more data will be received.
     *
     * The reason I suspect a timeout loop is because the CPU usage drops to (almost) zero.
     *
     * After disconnecting the putty window, the issue resolves itself. The TCP stack gets out of its timeout loop and the SC20 resumes operation as normal
     * In a bigger system, this complete system hangup waiting for the TCP socket has caused issues like running out of memory or queues overflowing.
     */

    internal class Program
    {
        private static TcpSocket socket;

        static void Main()
        {
            Network.Initialize("192.168.31.55", "255.255.255.0",
                "192.168.31.254", "192.168.31.254", new byte[]{ 0x80, 0x1F, 0x12, 0xEE, 0xB6, 0xD9 });

            socket = new TcpSocket(6161);
            socket.Start();

            new Thread(PrintCpu).Start();

            while (true)
            {
                socket.SendMessage("$GNGGA,001043.00,4404.14036,N,12118.85961,W,1,12,0.98,1113.0,M,-21.3,M*47\r\n");
                Thread.Sleep(10); //Decreasing this timer will fix the issue.
            }

        }

        private static void PrintCpu()
        {
            while (true)
            {
                //Should be around 26% when putty isnt connected
                //When putty is connected, it will drop to 0% and will not be printed 1x per second but much slower.
                Debug.WriteLine($"{DeviceInformation.GetCpuUsageStatistic():N0}%");
                Thread.Sleep(1000);
            }
        }

    }
}
