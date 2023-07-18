using NetworkPingTool.Model.RequestObjects;
using NetworkPingTool.ViewModels;

namespace NetworkPingTool.Services.PingApiService
{
    public class PingApiService : IPingApiService
    {
        private readonly IHttpClientFactory httpClientFactory;

        public PingApiService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<bool> StartPingingAddressAsync(PingingIpAddressViewModel ipAddress)
        {
            var client = httpClientFactory.CreateClient("API");
            var result = await client.PostAsJsonAsync("/ping/pingOne", new StartPingingAddressRequest { IpAddress = ipAddress.IpAddress });
            return result.IsSuccessStatusCode;
        }

        public async Task<bool> StartPingingAddressesAsync(IEnumerable<PingingIpAddressViewModel> ipAddresses)
        {
            var client = httpClientFactory.CreateClient("API");
            var result = await client.PostAsJsonAsync("/ping/pingMany", new StartPingingAddressesRequest { IpAddresses = ipAddresses.Select(ip => ip.IpAddress) });
            return result.IsSuccessStatusCode;
        }

        public async Task<bool> StopPingingAddressAsync(PingingIpAddressViewModel ipAddress)
        {
            var client = httpClientFactory.CreateClient("API");
            var result = await client.PostAsJsonAsync("/ping/stop", new StopPingingAddressesRequest { IpAddresses = new[] { ipAddress.IpAddress } });
            return result.IsSuccessStatusCode;
        }

        public async Task<bool> StopPingingAllAddressesAsync()
        {
            var client = httpClientFactory.CreateClient("API");
            var result = await client.PostAsJsonAsync("/ping/stopAll", new StopPingingAllAddressesRequest());
            return result.IsSuccessStatusCode;
        }

        public async Task<HttpResponseMessage> UpdatePingInterval(int intervalMilliseconds)
        {
            var client = httpClientFactory.CreateClient("API");
            var result = await client.PostAsJsonAsync("/ping/interval", new UpdatePingIntervalRequest { IntervalMilliseconds = intervalMilliseconds });
            return result;
        }
    }
}
