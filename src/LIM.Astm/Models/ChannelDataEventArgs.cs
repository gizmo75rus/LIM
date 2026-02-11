namespace LIM.Astm.Models;

public class ChannelDataEventArgs : EventArgs
{
    public string DataLine { get; set; }

    public ChannelDataEventArgs(string dataLine)
    {
        DataLine = dataLine;
    }
}