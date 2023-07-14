using NetworkPingTool.Model;

namespace NetworkPingTool.Services.PingHealthService
{
    public interface IPingHealthService
    {
        PingHealthStatus GetHealthStatus(IEnumerable<PingResult> pings, bool isDns);
    }
}
