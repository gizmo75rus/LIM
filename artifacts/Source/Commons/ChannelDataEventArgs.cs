using System;

namespace AstmDriver.Commons
{
    public class ChannelDataEventArgs : EventArgs
    { 
        public string Data { get; set; }

        public string RawData { get; set; }

        public ChannelDataEventArgs(string dataLine)
        {
            Data = dataLine;
        }

        public ChannelDataEventArgs(string dataLine, string rawData) : this(dataLine)
        {
            RawData = rawData;
        }
    }
}
