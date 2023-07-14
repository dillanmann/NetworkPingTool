using Microsoft.AspNetCore.SignalR.Client;
using NetworkPingTool.Model;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using NetworkPingTool.Model.RequestObjects;
using NetworkPingTool.Shared.Validators;

namespace NetworkPingTool.ViewModels
{
    public class IndexViewModel : BaseViewModel
    {
        private HubConnection hubConnection;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly List<PingResult> pingResults = new();

        public IndexViewModel(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
            Console.WriteLine($"In {nameof(IndexViewModel)} constructor");
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
            IpAddresses.Remove(ipAddress);
        }

        public async Task DeleteAllPingingAddresses()
        {
            await StopPingingAllAddresses();
            IpAddresses.Clear();
        }

        public async Task StartPingingAddress(PingingIpAddress ipAddress)
        {
            var client = httpClientFactory.CreateClient("API");
            var result = await client.PostAsJsonAsync("/ping/pingOne", new StartPingingAddressRequest { IpAddress = ipAddress.IpAddress });
            if (result.IsSuccessStatusCode)
            {
                ipAddress.IsActive = true;
            }
        }

        public async Task StopPingingAddress(PingingIpAddress ipAddress)
        {
            var client = httpClientFactory.CreateClient("API");
            var result = await client.PostAsJsonAsync("/ping/stop", new StopPingingAddressesRequest { IpAddresses = new[] { ipAddress.IpAddress } });
            if (result.IsSuccessStatusCode)
            {
                ipAddress.IsActive = false;
            }
        }

        public async Task StopPingingAllAddresses()
        {
            var client = httpClientFactory.CreateClient("API");
            var result = await client.PostAsJsonAsync("/ping/stopAll", new StopPingingAllAddressesRequest());
            if (result.IsSuccessStatusCode)
            {
                foreach (var pingingAddress in IpAddresses)
                {
                    pingingAddress.IsActive = false;
                }
            }
        }

        public override async Task OnInitializedAsync()
        {
            Console.WriteLine($"In {nameof(IndexViewModel)} {nameof(OnInitializedAsync)}");

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

        private async Task HubConnection_Closed(Exception arg)
        {
            await hubConnection.StartAsync();
        }

        public async ValueTask DisposeAsync()
        {
            Console.WriteLine($"In {nameof(IndexViewModel)} {nameof(DisposeAsync)}");
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

        public int TotalFailures { get; set; }
    }
}
