using Microsoft.AspNetCore.Mvc;
using NetworkPingTool.Model.RequestObjects;
using NetworkPingTool.Services;
using System.Net;

namespace NetworkPingTool.Controllers
{
    [Controller]
    [Route("[controller]")]
    public class PingController : ControllerBase
    {
        private readonly IPingAddressService pingAddressService;

        public PingController(IPingAddressService pingAddressService)
        {
            this.pingAddressService = pingAddressService;
        }

        [HttpPost]
        [Route("pingOne")]
        public IActionResult StartPingingAddress([FromBody] StartPingingAddressRequest request)
        {
            if (!IPAddress.TryParse(request.IpAddress, out var address))
            {
                return BadRequest($"Address {request.IpAddress} not valid");
            }

            pingAddressService.StartPingingAddress(address);
            return Ok();
        }

        [HttpPost]
        [Route("pingMany")]
        public IActionResult StartPingingAddresses([FromBody] StartPingingAddressesRequest request)
        {
            var parsedAddresses = request.IpAddresses.Select(ip =>
            {
                if (IPAddress.TryParse(ip, out var address))
                {
                    return address;
                }

                return null;
            });

            if (parsedAddresses.Any(ipaddr => ipaddr == null))
            {
                return BadRequest($"One or more addresses not valid");
            }

            pingAddressService.StartPingingAddresses(parsedAddresses);
            return Ok();
        }

        [HttpPost]
        [Route("stop")]
        public IActionResult StopPingingAddresses([FromBody] StopPingingAddressesRequest request)
        {
            var parsedAddresses = request.IpAddresses.Select(ip =>
            {
                if (IPAddress.TryParse(ip, out var address))
                {
                    return address;
                }

                return null;
            });

            if (parsedAddresses.Any(ipaddr => ipaddr == null))
            {
                return BadRequest($"One or more addresses not valid");
            }

            pingAddressService.StopPingingAddresses(parsedAddresses);
            return Ok();
        }

        [HttpPost]
        [Route("stopAll")]
        public IActionResult StopPingingAllAddresses([FromBody] StopPingingAllAddressesRequest request)
        {
            pingAddressService.StopPingingAllAddresses();
            return Ok();
        }

        [HttpPost]
        [Route("interval")]
        public IActionResult UpdatePingInterval([FromBody] UpdatePingIntervalRequest request)
        {
            if (request.IntervalMilliseconds < 0)
            {
                return BadRequest("Interval can't be negative");
            }

            pingAddressService.SetPingInterval(request.IntervalMilliseconds);
            return Ok();
        }
    }
}
