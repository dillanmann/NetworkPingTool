namespace NetworkPingTool.Model
{
    public record CreateConnectionEventArgs (string IpAddress, string Label, bool IsDns);
}
