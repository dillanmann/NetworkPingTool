using Microsoft.AspNetCore.SignalR.Client;
using NetworkPingTool.Model;
using System.Text.Json;
using NetworkPingTool.Shared.Validators;
using NetworkPingTool.Services.NotifySettingsChangedService;
using NetworkPingTool.Services.PingApiService;
using NetworkPingTool.Services.PingHealthService;

namespace NetworkPingTool.ViewModels
{
    public class IndexViewModel : BaseViewModel
    {
        private HubConnection hubConnection;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly INotifySettingsChangedService notifySettingsChangedService;
        private readonly IPingApiService pingApiService;
        private readonly IPingHealthService pingHealthService;
        private readonly List<PingResult> pingResults = new();

        public IndexViewModel(
            IHttpClientFactory httpClientFactory,
            INotifySettingsChangedService notifySettingsChangedService,
            IPingApiService pingApiService,
            IPingHealthService pingHealthService)
        {
            this.httpClientFactory = httpClientFactory;
            this.notifySettingsChangedService = notifySettingsChangedService;
            this.pingApiService = pingApiService;
            this.pingHealthService = pingHealthService;
            this.notifySettingsChangedService.SettingsChanged += OnSettingsChanged;
        }

        public List<PingingIpAddress> IpAddresses { get; set; } = new List<PingingIpAddress>();

        public string NewAddressToAdd { get; set; }

        public bool CanAddNewIpAddress { get => IpAddressValidator.IpAddressIsValid(NewAddressToAdd) && !IpAddresses.Any(ip => ip.IpAddress == NewAddressToAdd); }

        public bool CanDeleteAllPingingAddresses { get => IpAddresses.Any() && IpAddresses.All(ip => ip.IsActive == false); }

        public bool CanStopAllPingingAddresses { get => IpAddresses.Any(ip => ip.IsActive); }

        public event Func<Task> NotifyStateChange;

        public void AddNewIpAddress()
        {
            IpAddresses.Add(new PingingIpAddress() { IpAddress = NewAddressToAdd });
            NewAddressToAdd = null;
        }

        public async Task DeleteIpAddress(PingingIpAddress ipAddress)
        {
            await StopPingingAddress(ipAddress);
            pingResults.RemoveAll(p => p.IpAddress == ipAddress.IpAddress);
            IpAddresses.Remove(ipAddress);
        }

        public async Task DeleteAllPingingAddresses()
        {
            await StopPingingAllAddresses();
            pingResults.Clear();
            IpAddresses.Clear();
        }

        public async Task StartPingingAddress(PingingIpAddress ipAddress)
        {
            var result = await pingApiService.StartPingingAddressAsync(ipAddress);
            if (result)
            {
                ipAddress.IsActive = true;
            }
        }

        public async Task StopPingingAddress(PingingIpAddress ipAddress)
        {
            var result = await pingApiService.StopPingingAddressAsync(ipAddress);
            if (result)
            {
                ipAddress.IsActive = false;
            }
        }

        public async Task StopPingingAllAddresses()
        {
            var result = await pingApiService.StopPingingAllAddressesAsync();
            if (result)
            {
                foreach (var pingingAddress in IpAddresses)
                {
                    pingingAddress.IsActive = false;
                }
            }
        }

        public override async Task OnInitializedAsync()
        {
            if (hubConnection != null) { }
            var client = httpClientFactory.CreateClient("API");
            hubConnection = new HubConnectionBuilder()
                .WithUrl($"{client.BaseAddress}pingresulthub")
                .Build();

            hubConnection.On<string>("SendPingResult", async (pingReplyJson) =>
            {
                var pingResult = JsonSerializer.Deserialize<PingResult>(pingReplyJson);
                await OnNewPingResult(pingResult);
            });

            hubConnection.Closed += HubConnection_Closed;
            await hubConnection.StartAsync();
        }

        private async void OnSettingsChanged(object sender, EventArgs e)
        {
            var activePingingAddresses = IpAddresses.Where(ip => ip.IsActive).ToList();
            if (!activePingingAddresses.Any())
            {
                return;
            }

            await StopPingingAllAddresses();
            await StartPingingAddresses(activePingingAddresses);
        }

        private async Task StartPingingAddresses(IEnumerable<PingingIpAddress> ipAddresses)
        {
            var result = await pingApiService.StartPingingAddressesAsync(ipAddresses);
            if (result)
            {
                foreach (var pingingAddress in IpAddresses)
                {
                    pingingAddress.IsActive = true;
                }
            }
        }

        private async Task HubConnection_Closed(Exception arg)
        {
            await hubConnection.StartAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (hubConnection is not null)
            {
                await hubConnection.DisposeAsync();
            }
        }

        private async Task OnNewPingResult(PingResult result)
        {
            pingResults.Add(result);
            var pingingIpAddress = IpAddresses.First(ip => ip.IpAddress == result.IpAddress);

            pingingIpAddress.TotalPings += 1;
            if (!result.Success)
            {
                pingingIpAddress.TotalFailures += 1;
            }
            var resultsForIp = pingResults.Where(ip => ip.IpAddress == result.IpAddress);
            pingingIpAddress.MaxRoundTripTime = resultsForIp.Max(r => r.RoundtripTime);
            pingingIpAddress.MinRoundTripTime = resultsForIp.Min(r => r.RoundtripTime);
            pingingIpAddress.AverageRoundTripTime = (long)resultsForIp.Average(r => r.RoundtripTime);
            pingingIpAddress.CurrentRoundTripTime = result.RoundtripTime;
            pingingIpAddress.HealthStatus = pingHealthService.GetHealthStatus(resultsForIp, pingingIpAddress.IsDnsAddress);
            pingingIpAddress.Results = resultsForIp;

            await NotifyStateChange?.Invoke();
        }
    }

    public class PingingIpAddress
    {
        public string IpAddress { get; set; }

        public string Label { get; set; }

        public bool IsActive { get; set; }

        public long MinRoundTripTime { get; set; }

        public long MaxRoundTripTime { get; set; }

        public long AverageRoundTripTime { get; set; }

        public long CurrentRoundTripTime { get; set; }

        public PingHealthStatus HealthStatus { get; set; }

        public bool IsDnsAddress { get; set; }

        public int TotalPings { get; set; }

        public int TotalFailures { get; set; }

        public IEnumerable<PingResult> Results { get; set; }
    }
}
