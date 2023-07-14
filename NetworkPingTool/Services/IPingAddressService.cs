using System.Net;

namespace NetworkPingTool.Services
{
    public interface IPingAddressService
    {
        void StartPingingAddress(IPAddress address);
        void StartPingingAddresses(IEnumerable<IPAddress> addresses);
        void StopPingingAddresses(IEnumerable<IPAddress> addresses);
        void StopPingingAllAddresses();
    }
}