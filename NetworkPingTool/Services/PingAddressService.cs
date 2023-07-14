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
        private readonly Dictionary<string, RunningPingTask> activeTasks = new();
        private readonly IHubContext<PingResultHub> pingResultHub;
        private const int pingIntervalMillis = 500;

        public PingAddressService(IHubContext<PingResultHub> pingResultHub)
        {
            this.pingResultHub = pingResultHub;
        }

        public void StartPingingAddresses(IEnumerable<IPAddress> addresses)
        {
            foreach (var address in addresses)
            {
                var tokenSource = new CancellationTokenSource();
                activeTasks.Add(address.ToString(), new RunningPingTask(NewPingAddressTask(address, tokenSource.Token), tokenSource));
            }
        }

        public void StartPingingAddress(IPAddress address)
        {
            var tokenSource = new CancellationTokenSource();
            activeTasks.Add(address.ToString(), new RunningPingTask(NewPingAddressTask(address, tokenSource.Token), tokenSource));
        }

        public void StopPingingAllAddresses()
        {
            foreach (var pair in activeTasks)
            {
                var runningTask = pair.Value;
                if (runningTask.CancellationTokenSource.IsCancellationRequested) continue;
                runningTask.CancellationTokenSource.Cancel();
            }
            activeTasks.Clear();
        }

        public void StopPingingAddresses(IEnumerable<IPAddress> addresses)
        {
            var tasksToRemove = new List<string>();
            foreach (var pair in activeTasks
                .Where(t => addresses.Select(a => a.ToString()).Contains(t.Key)))
            {
                var runningTask = pair.Value;
                if (runningTask.CancellationTokenSource.IsCancellationRequested) continue;
                runningTask.CancellationTokenSource.Cancel();
                tasksToRemove.Add(pair.Key);
            }

            foreach (var taskToRemove in tasksToRemove)
            {
                activeTasks.Remove(taskToRemove);
            }
        }

        private async Task PingUntilCancelled(IPAddress address, CancellationToken token)
        {
            var pingSender = new Ping();
            var result = await pingSender.SendPingAsync(address);
            if (token.IsCancellationRequested) return;

            await pingResultHub.Clients.All.SendAsync(
                PingResultHub.PingResultMessageMethodName, JsonSerializer.Serialize(NewPingResult(result, address)), token);


            using (var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(pingIntervalMillis)))
            {
                while (await timer.WaitForNextTickAsync(token))
                {
                    if (token.IsCancellationRequested) return;
                    result = await pingSender.SendPingAsync(address);

                    await pingResultHub.Clients.All.SendAsync(
                        PingResultHub.PingResultMessageMethodName, JsonSerializer.Serialize(NewPingResult(result, address)), token);
                }
            }
        }

        private Task NewPingAddressTask(IPAddress address, CancellationToken token)
            => Task.Run(async () => await PingUntilCancelled(address, token), token);

        private static PingResult NewPingResult(PingReply reply, IPAddress originAddress = null)
            => new()
            {
                IpAddress = originAddress.ToString() ?? reply.Address.ToString(),
                Status = reply.Status,
                RoundtripTime = reply.RoundtripTime
            };
    }

    public class RunningPingTask
    {
        public RunningPingTask(Task activeTask, CancellationTokenSource cancellationTokenSource)
        {
            CancellationTokenSource = cancellationTokenSource;
            ActiveTask = activeTask;
        }

        public CancellationTokenSource CancellationTokenSource { get; }
        public Task ActiveTask { get; }
    }
}
