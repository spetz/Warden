using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Warden.Watchers.Performance
{
    public interface IPerformance
    {
        Task<ResourceUsage> GetResourceUsageAsync();
    }

    public class Performance : IPerformance
    {
        private readonly TimeSpan _delay;

        public Performance(TimeSpan delay)
        {
            _delay = delay;
        }

        public async Task<ResourceUsage> GetResourceUsageAsync()
        {
            var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            var ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            cpuCounter.NextValue();
            ramCounter.NextValue();
            await Task.Delay(_delay);
            var cpuUsage = cpuCounter.NextValue();
            var ramUsage = ramCounter.NextValue();

            return ResourceUsage.Create(cpuUsage, ramUsage);
        }
    }
}