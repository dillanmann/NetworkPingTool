using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using NetworkPingTool.Model;
using NetworkPingTool.Services.NotifySettingsChangedService;
using NetworkPingTool.Services.PingApiService;
using NetworkPingTool.Services.PingHealthService;
using NetworkPingTool.Shared.Validators;
using System.Text.Json;

namespace NetworkPingTool.ViewModels
{
    public class IndexViewModel : BaseViewModel, IAsyncDisposable
    {
        private HubConnection hubConnection;
        private readonly INotifySettingsChangedService notifySettingsChangedService;
        private readonly IPingApiService pingApiService;
        private readonly IPingHealthService pingHealthService;
        private readonly IConfiguration configuration;
        private int pingRecordsToStore = 100;

        public IndexViewModel(
            INotifySettingsChangedService notifySettingsChangedService,
            IPingApiService pingApiService,
            IPingHealthService pingHealthService,
            IConfiguration configuration)
        {
            this.notifySettingsChangedService = notifySettingsChangedService;
            this.pingApiService = pingApiService;
            this.pingHealthService = pingHealthService;
            this.configuration = configuration;
            this.notifySettingsChangedService.SettingsChanged += OnSettingsChanged;
        }

        public List<PingingIpAddressViewModel> IpAddresses { get; set; } = new List<PingingIpAddressViewModel>();

        public string NewAddressToAdd { get; set; }

        public bool CanAddNewIpAddress { get => IpAddressValidator.IpAddressIsValid(NewAddressToAdd) && !IpAddresses.Any(ip => ip.IpAddress == NewAddressToAdd); }

        public bool CanDeleteAllPingingAddresses { get => IpAddresses.Any() && IpAddresses.All(ip => ip.IsActive == false); }

        public bool CanStopAllPingingAddresses { get => IpAddresses.Any(ip => ip.IsActive); }

        public event Func<Task> NotifyStateChange;

        public void AddNewIpAddress()
        {
            IpAddresses.Add(new PingingIpAddressViewModel(pingHealthService, pingRecordsToStore) { IpAddress = NewAddressToAdd });
            NewAddressToAdd = null;
        }

        public async Task DeleteIpAddress(PingingIpAddressViewModel ipAddress)
        {
            await StopPingingAddress(ipAddress);
            IpAddresses.Remove(ipAddress);
        }

        public async Task DeleteAllPingingAddresses()
        {
            await StopPingingAllAddresses();
            IpAddresses.Clear();
        }

        public async Task StartPingingAddress(PingingIpAddressViewModel ipAddress)
        {
            var result = await pingApiService.StartPingingAddressAsync(ipAddress);
            if (result)
            {
                ipAddress.IsActive = true;
            }
        }

        public async Task StopPingingAddress(PingingIpAddressViewModel ipAddress)
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
            var baseAddress = configuration.GetValue<string>("ApiRootUrl");
            hubConnection = new HubConnectionBuilder()
                .WithUrl($"{baseAddress}/pingresulthub")
                .Build();

            hubConnection.On<string>("SendPingResult", async (pingReplyJson) =>
            {
                var pingResult = JsonSerializer.Deserialize<PingResult>(pingReplyJson);
                await OnNewPingResult(pingResult);
            });

            hubConnection.Closed += HubConnection_Closed;
            await hubConnection.StartAsync();
        }

        public void OnKeyUp(KeyboardEventArgs keyboardEventArgs)
        {
            if (keyboardEventArgs.Key == "Enter" && CanAddNewIpAddress)
            {
                AddNewIpAddress();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (hubConnection is not null)
            {
                await hubConnection.DisposeAsync();
                GC.SuppressFinalize(this);
            }
        }

        private async void OnSettingsChanged(object sender, SettingsChangedEventArgs settingsEventArgs)
        {
            if (settingsEventArgs.TotalPingsToStore.HasValue)
            {
                PropogatePingRecordsToStoreChanged(settingsEventArgs.TotalPingsToStore.Value);
            }

            var activePingingAddresses = IpAddresses.Where(ip => ip.IsActive).ToList();
            if (!activePingingAddresses.Any())
            {
                return;
            }

            // Ping interval changed so need to restart running tasks
            if (settingsEventArgs.PingIntervalMillis.HasValue)
            {
                await StopPingingAllAddresses();
                await StartPingingAddresses(activePingingAddresses);
            }
        }

        private async Task StartPingingAddresses(IEnumerable<PingingIpAddressViewModel> ipAddresses)
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

        private async Task OnNewPingResult(PingResult result)
        {
            var pingingIpAddress = IpAddresses.First(ip => ip.IpAddress == result.IpAddress);
            await pingingIpAddress.AddNewResult(result);

            await NotifyStateChange?.Invoke();
        }

        private void PropogatePingRecordsToStoreChanged(int recordsToStore)
        {
            this.pingRecordsToStore = recordsToStore;
            foreach (var address in IpAddresses)
            {
                address.UpdateRecordsToStore(recordsToStore);
            }
        }
    }
}
