using NetworkPingTool.Model;

namespace NetworkPingTool.Services.PingHealthService
{
    public class PingHealthService : IPingHealthService
    {
        public int HealthyThresholdMillis { get; } = 60;
        public int UnhealthyThresholdMillis { get; } = 100;

        public int DnsHealthyThresholdMillis { get; } = 30;
        public int DnsUnhealthyThresholdMillis { get; } = 50;

        public PingHealthStatus GetHealthStatus(IEnumerable<PingResult> pings, bool isDns)
        {
            var average = pings.Average(p => p.RoundtripTime);

            // Special case for 0 results, all of which are failures
            if (average == 0 && pings.Any(p => p.Success == false))
            {
                return PingHealthStatus.Red;
            }

            if (isDns)
            {
                return GetDnsHealthStatus(average);
            }

            return GetPingHealthStatus(average);

        }

        private PingHealthStatus GetDnsHealthStatus(double average)
        {
            if (average > DnsUnhealthyThresholdMillis)
            {
                return PingHealthStatus.Red;
            }

            if (average > DnsHealthyThresholdMillis)
            {
                return PingHealthStatus.Amber;
            }

            return PingHealthStatus.Green;
        }

        private PingHealthStatus GetPingHealthStatus(double average)
        {
            if (average > UnhealthyThresholdMillis)
            {
                return PingHealthStatus.Red;
            }

            if (average > HealthyThresholdMillis)
            {
                return PingHealthStatus.Amber;
            }

            return PingHealthStatus.Green;
        }
    }
}
