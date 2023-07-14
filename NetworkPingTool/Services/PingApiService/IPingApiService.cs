using NetworkPingTool.ViewModels;

namespace NetworkPingTool.Services.PingApiService
{
    public interface IPingApiService
    {
        Task<bool> StartPingingAddressAsync(PingingIpAddress ipAddress);
        Task<bool> StartPingingAddressesAsync(IEnumerable<PingingIpAddress> ipAddress);
        Task<bool> StopPingingAddressAsync(PingingIpAddress ipAddress);
        Task<bool> StopPingingAllAddressesAsync();
    }
}
