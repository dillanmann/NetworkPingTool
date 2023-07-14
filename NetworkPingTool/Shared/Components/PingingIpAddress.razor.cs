using Microsoft.AspNetCore.Components;
using MudBlazor;
using NetworkPingTool.Model;

namespace NetworkPingTool.Shared.Components
{
    public partial class PingingIpAddress : MudComponentBase
    {
        private bool isDns;
        private string label;

        public string IpAddressClasses { get => IsActive ? "active-address" : ""; }

        public bool IsDns
        {
            get => isDns;
            set
            {
                isDns = value;
                OnDnsChanged.InvokeAsync(value);
            }
        }

        public string Label
        {
            get => label;
            set
            {
                label = value;
                OnLabelChanged.InvokeAsync(value);
            }
        }

        [Parameter]
        public string IpAddress { get; set; }

        [Parameter]
        public bool IsActive { get; set; }

        [Parameter]
        public long MinRoundTripTime { get; set; }

        [Parameter]
        public long MaxRoundTripTime { get; set; }

        [Parameter]
        public long AverageRoundTripTime { get; set; }

        [Parameter]
        public long CurrentRoundTripTime { get; set; }

        [Parameter]
        public long TotalPings { get; set; }

        [Parameter]
        public long TotalFailures { get; set; }

        [Parameter]
        public EventCallback<bool> OnDnsChanged { get; set; }

        [Parameter]
        public EventCallback<string> OnLabelChanged { get; set; }

        [Parameter]
        public PingHealthStatus HealthStatus { get; set; }

        [Parameter]
        public EventCallback OnPlayClicked { get; set; }

        [Parameter]
        public EventCallback OnStopClicked { get; set; }

        [Parameter]
        public EventCallback OnDeleteClicked { get; set; }

        private async Task InvokeButtonClick()
        {
            if (IsActive)
            {
                await OnStopClicked.InvokeAsync();
            }
            else
            {
                await OnPlayClicked.InvokeAsync();
            }
        }
    }
}
