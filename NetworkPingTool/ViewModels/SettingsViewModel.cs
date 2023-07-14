using MudBlazor;
using NetworkPingTool.Model.RequestObjects;
using NetworkPingTool.Services.NotifySettingsChangedService;

namespace NetworkPingTool.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ISnackbar snackbar;
        private readonly INotifySettingsChangedService notifySettingsChangedService;
        private int previousSavedPingInterval = 500;

        public SettingsViewModel(IHttpClientFactory httpClientFactory, ISnackbar snackbar, INotifySettingsChangedService notifySettingsChangedService)
        {
            this.httpClientFactory = httpClientFactory;
            this.snackbar = snackbar;
            this.notifySettingsChangedService = notifySettingsChangedService;
        }

        public int PingInterval { get; set; } = 500;

        public bool CanSave { get => PingInterval != previousSavedPingInterval; }

        public async Task SaveSettings()
        {
            var client = httpClientFactory.CreateClient("API");
            IsLoading = true;
            try
            {
                var result = await client.PostAsJsonAsync("/ping/interval", new UpdatePingIntervalRequest { IntervalMilliseconds = PingInterval });
                if (result.IsSuccessStatusCode)
                {
                    previousSavedPingInterval = PingInterval;
                    notifySettingsChangedService.EmitSettingsChanged();
                    snackbar.Add("Settings saved", Severity.Success);
                }
                else
                {
                    snackbar.Add($"Failed to save settings: {result.StatusCode} {result.ReasonPhrase}", Severity.Error);
                }
            }
            catch(Exception ex)
            {
                snackbar.Add($"Failed to save settings: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
