namespace NetworkPingTool.Services.NotifySettingsChangedService
{
    public interface INotifySettingsChangedService
    {
        public event EventHandler SettingsChanged;

        void EmitSettingsChanged();
    }
}
