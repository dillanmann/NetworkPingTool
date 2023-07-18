using MudBlazor;
using NetworkPingTool.Model;
using NetworkPingTool.Services.NotifySettingsChangedService;
using NetworkPingTool.Services.PingApiService;
using NetworkPingTool.ViewModels.Helpers;

namespace NetworkPingTool.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ISnackbar snackbar;
        private readonly INotifySettingsChangedService notifySettingsChangedService;
        private readonly IPingApiService pingApiService;

        public SettingsViewModel(ISnackbar snackbar, INotifySettingsChangedService notifySettingsChangedService, IPingApiService pingApiService)
        {
            this.snackbar = snackbar;
            this.notifySettingsChangedService = notifySettingsChangedService;
            this.pingApiService = pingApiService;
        }

        public Setting<int> PingInterval { get; set; } = new Setting<int> { Value = 500 };
        public Setting<int> TotalPingsToStore { get; set; } = new Setting<int> { Value = 100 };

        public bool CanSave { get => PingInterval.HasChanged || TotalPingsToStore.HasChanged; }

        public async Task SaveSettings()
        {
            IsLoading = true;
            var success = false;
            try
            {
                var settingsEventArgs = new SettingsChangedEventArgs();
                if (PingInterval.HasChanged)
                {
                    var result = await SavePingInterval();
                    if (result.IsSuccessStatusCode)
                    {
                        success = true;
                        settingsEventArgs.PingIntervalMillis = PingInterval.Value;
                    }
                    else
                    {
                        snackbar.Add($"Failed to save settings: {result.StatusCode} {result.ReasonPhrase}", Severity.Error);
                    }
                }

                if (TotalPingsToStore.HasChanged)
                {
                    TotalPingsToStore.SaveChanges();
                    settingsEventArgs.TotalPingsToStore = TotalPingsToStore.Value;
                    success = true;
                }

                if (success)
                {
                    snackbar.Add($"Settings saved", Severity.Success);
                    notifySettingsChangedService.EmitSettingsChanged(settingsEventArgs);
                }
            }
            catch (Exception ex)
            {
                snackbar.Add($"Failed to save settings: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task<HttpResponseMessage> SavePingInterval()
        {
            var result = await pingApiService.UpdatePingInterval(PingInterval.Value);
            if (result.IsSuccessStatusCode)
            {
                PingInterval.SaveChanges();
            }

            return result;
        }
    }
}
