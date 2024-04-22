using System;

namespace TinyCLRApplication1
{
    public class DataReceivedEventArgs : EventArgs
    {
        public byte[] DataBytes { get; set; }
        public string Data { get; set; }
    }
}