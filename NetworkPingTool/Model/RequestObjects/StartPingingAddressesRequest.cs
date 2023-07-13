namespace NetworkPingTool.Model.RequestObjects
{
    public class StartPingingAddressesRequest
    {
        public IEnumerable<string> IpAddresses { get; set; }
    }
}
