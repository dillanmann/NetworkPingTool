namespace NetworkPingTool.Services.NotifySettingsChangedService
{
    public class NotifySettingsChangedService : INotifySettingsChangedService
    {
        public event EventHandler SettingsChanged;

        public void EmitSettingsChanged()
        {
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
