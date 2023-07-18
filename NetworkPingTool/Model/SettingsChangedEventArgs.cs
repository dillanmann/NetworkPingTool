namespace NetworkPingTool.Model
{
    public class SettingsChangedEventArgs
    {
        public int? PingIntervalMillis { get; set; } = null;
        public int? TotalPingsToStore { get; set; } = null;
    }
}
