using System.Net.NetworkInformation;

namespace NetworkPingTool.Model
{
    public record PingResult
    {
        public string IpAddress { get; init; }

        public IPStatus Status { get; init; }

        public long RoundtripTime { get; init; }

        public bool Success { get => Status == IPStatus.Success; }

        public DateTime TimeCompleted { get; init; }
    }
}
