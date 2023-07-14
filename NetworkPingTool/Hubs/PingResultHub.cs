using Microsoft.AspNetCore.SignalR;

namespace NetworkPingTool.Hubs
{
    public class PingResultHub : Hub
    {
        public const string PingResultMessageMethodName = "SendPingResult";
    }
}
