using Microsoft.AspNetCore.SignalR.Client;
using NetworkPingTool.Model;
using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace NetworkPingTool.ViewModels
{
    public class IndexViewModel : BaseViewModel
    {
        private HubConnection hubConnection;
        private readonly NavigationManager navigation;
        private readonly List<PingResult> pingResults = new();

        public IndexViewModel(NavigationManager navigation)
        {
            this.navigation = navigation;
        }

        public List<PingingIpAddress> IpAddresses { get; set; } = new List<PingingIpAddress>();

        public event Func<Task> NotifyStateChange;

        public void AddNewIpAddress(PingingIpAddress ipAddress)
        {
            IpAddresses.Add(ipAddress);
            // do other stuff too
        }

        public void DeleteIpAddress(PingingIpAddress ipAddress)
        {
            IpAddresses.Remove(ipAddress);
            // do other stuff too
        }

        public void StartPingingAddress(PingingIpAddress ipAddress)
        {
            ipAddress.IsActive = true;
        }

        public void StopPingingAddress(PingingIpAddress ipAddress)
        {
            ipAddress.IsActive = false;
        }

        public override async Task OnInitializedAsync()
        {
            IpAddresses.Add(new PingingIpAddress() { IpAddress = "192.168.0.170" });
            hubConnection = new HubConnectionBuilder()
                .WithUrl(navigation.ToAbsoluteUri("/pingresulthub"))
                .Build();

            hubConnection.On<string>("SendPingResult", async (pingReplyJson) =>
            {
                var pingResult = JsonSerializer.Deserialize<PingResult>(pingReplyJson);
                await OnNewPingResult(pingResult);
            });

            await hubConnection.StartAsync();
        }

        private async Task OnNewPingResult(PingResult result)
        {
            pingResults.Add(result);
            var pingingIpAddress = IpAddresses.First(ip => ip.IpAddress == result.IpAddress);

            pingingIpAddress.TotalPings += 1;
            var resultsForIp = pingResults.Where(ip => ip.IpAddress == result.IpAddress);
            pingingIpAddress.MaxRoundTripTime = resultsForIp.Max(r => r.RoundtripTime);
            pingingIpAddress.MinRoundTripTime = resultsForIp.Min(r => r.RoundtripTime);
            pingingIpAddress.AverageRoundTripTime = (long)resultsForIp.Average(r => r.RoundtripTime);

            await NotifyStateChange?.Invoke();
        }
    }

    public class PingingIpAddress
    {
        public string IpAddress { get; set; }

        public bool IsActive { get; set; }

        public long MinRoundTripTime { get; set; }

        public long MaxRoundTripTime { get; set; }

        public long AverageRoundTripTime { get; set; }

        public int TotalPings { get; set; }
    }
}
