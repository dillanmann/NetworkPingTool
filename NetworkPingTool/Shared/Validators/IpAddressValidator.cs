using System.Net;

namespace NetworkPingTool.Shared.Validators
{
    public class IpAddressValidator
    {
        // Use in viewmodels
        public static IEnumerable<string> ValidateIpAddress(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                yield return "IP address cannot be empty";
            }

            if (!IpAddressIsValid(ipAddress))
            {
                yield return "IP address is in the wrong format";
            }
        }

        public static bool IpAddressIsValid(string ipAddress) => IPAddress.TryParse(ipAddress, out _);

        // Use on MudBlazor components
        public static Func<string, IEnumerable<string>> Validate => new(ValidateIpAddress);
    }
}
