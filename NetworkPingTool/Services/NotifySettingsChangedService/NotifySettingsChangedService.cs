using NetworkPingTool.Model;

namespace NetworkPingTool.Services.NotifySettingsChangedService
{
    public class NotifySettingsChangedService : INotifySettingsChangedService
    {
        public event EventHandler<SettingsChangedEventArgs> SettingsChanged;

        public void EmitSettingsChanged(SettingsChangedEventArgs settings) => SettingsChanged?.Invoke(this, settings);
    }
}
