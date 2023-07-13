using Microsoft.AspNetCore.SignalR;
using System.Net.NetworkInformation;
using System.Text.Json;

namespace NetworkPingTool.Hubs
{
    public class PingResultHub : Hub
    {
        public const string PingResultMessageMethodName = "SendPingResult";
        public async Task SendPingResultAsync(PingReply reply)
             => await Clients.All.SendAsync(PingResultMessageMethodName, JsonSerializer.Serialize(reply));
    }
}
