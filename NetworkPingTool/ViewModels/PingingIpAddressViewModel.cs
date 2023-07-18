﻿using NetworkPingTool.Model;
using NetworkPingTool.Services.PingHealthService;

namespace NetworkPingTool.ViewModels
{
    public class PingingIpAddressViewModel
    {
        private readonly IPingHealthService pingHealthService;
        private int recordsToStore;

        public PingingIpAddressViewModel(IPingHealthService pingHealthService, int pingRecordsToStore)
        {
            this.pingHealthService = pingHealthService;
            this.recordsToStore = pingRecordsToStore;
        }

        public string IpAddress { get; set; }

        public string Label { get; set; }

        public bool IsActive { get; set; }

        public long MinRoundTripTime { get; set; }

        public long MaxRoundTripTime { get; set; }

        public long AverageRoundTripTime { get; set; }

        public long CurrentRoundTripTime { get; set; }

        public PingHealthStatus HealthStatus { get; set; }

        public bool IsDnsAddress { get; set; }

        public int TotalPings { get; set; }

        public int TotalFailures { get; set; }

        public IEnumerable<PingResult> Results { get; set; } = new List<PingResult>();

        public Task AddNewResult(PingResult result)
        {
            Results = Results.TakeLast(recordsToStore - 1).Append(result).ToList();

            TotalPings += 1;
            if (!result.Success)
            {
                TotalFailures += 1;
            }

            MaxRoundTripTime = Results.Max(r => r.RoundtripTime);
            MinRoundTripTime = Results.Min(r => r.RoundtripTime);
            AverageRoundTripTime = (long)Results.Average(r => r.RoundtripTime);
            CurrentRoundTripTime = result.RoundtripTime;
            HealthStatus = pingHealthService.GetHealthStatus(Results, IsDnsAddress);

            return Task.CompletedTask;
        }

        public void UpdateRecordsToStore(int pingRecordsToStore) => recordsToStore = pingRecordsToStore;
    }
}
