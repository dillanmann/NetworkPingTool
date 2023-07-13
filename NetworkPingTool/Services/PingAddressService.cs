using Microsoft.AspNetCore.SignalR;
using NetworkPingTool.Hubs;
using NetworkPingTool.Model;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;

namespace NetworkPingTool.Services
{
    public class PingAddressService : IPingAddressService
    {
        private readonly List<Task> activeTasks = new();
        private readonly IHubContext<PingResultHub> pingResultHub;
        private const int pingIntervalMillis = 500;
        private CancellationTokenSource pingTaskCancellationToken;

        public PingAddressService(IHubContext<PingResultHub> pingResultHub)
        {
            this.pingResultHub = pingResultHub;
        }

        public void StartPingingAddresses(IEnumerable<IPAddress> addresses)
        {
            EnsureCancellationTokenActive();

            foreach (var address in addresses)
            {
                activeTasks.Add(NewPingAddressTask(address));
            }
        }

        public void StartPingingAddress(IPAddress address)
        {
            EnsureCancellationTokenActive();

            activeTasks.Add(NewPingAddressTask(address));
        }

        public void StopPingingAllAddresses()
        {
            if (activeTasks.Any())
            {
                pingTaskCancellationToken.Cancel();
            }
        }

        private async Task PingUntilCancelled(IPAddress address, CancellationToken token)
        {
            var pingSender = new Ping();
            var result = await pingSender.SendPingAsync(address);
            await pingResultHub.Clients.All.SendAsync(PingResultHub.PingResultMessageMethodName, JsonSerializer.Serialize(NewPingResult(result)), token);

            using (var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(pingIntervalMillis)))
            {
                while (await timer.WaitForNextTickAsync(token))
                {
                    result = await pingSender.SendPingAsync(address);
                    await pingResultHub.Clients.All.SendAsync(PingResultHub.PingResultMessageMethodName, JsonSerializer.Serialize(NewPingResult(result)), token);
                }
            }
        }

        private void EnsureCancellationTokenActive()
        {
            if (!activeTasks.Any())
            {
                pingTaskCancellationToken = new CancellationTokenSource();
            }
        }

        private Task NewPingAddressTask(IPAddress address) => Task.Run(async () => await PingUntilCancelled(address, pingTaskCancellationToken.Token));

        private static PingResult NewPingResult(PingReply reply) => new(reply.Address.ToString(), reply.Status, reply.RoundtripTime);
    }
}
