using NetworkPingTool.Model;

namespace NetworkPingTool.Services.NotifySettingsChangedService
{
    public interface INotifySettingsChangedService
    {
        public event EventHandler<SettingsChangedEventArgs> SettingsChanged;

        void EmitSettingsChanged(SettingsChangedEventArgs settings);
    }
}
