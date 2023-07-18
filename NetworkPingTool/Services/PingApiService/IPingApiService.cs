using NetworkPingTool.ViewModels;

namespace NetworkPingTool.Services.PingApiService
{
    public interface IPingApiService
    {
        Task<bool> StartPingingAddressAsync(PingingIpAddressViewModel ipAddress);
        Task<bool> StartPingingAddressesAsync(IEnumerable<PingingIpAddressViewModel> ipAddress);
        Task<bool> StopPingingAddressAsync(PingingIpAddressViewModel ipAddress);
        Task<bool> StopPingingAllAddressesAsync();
        Task<HttpResponseMessage> UpdatePingInterval(int intervalMilliseconds);
    }
}
