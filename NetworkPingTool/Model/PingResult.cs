using System.Net.NetworkInformation;

namespace NetworkPingTool.Model
{
    public record PingResult(string IpAddress, IPStatus Status, long RoundtripTime);
}
