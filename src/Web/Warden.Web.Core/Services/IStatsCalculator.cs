using System;
using System.Collections.Generic;
using System.Linq;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Dto;

namespace Warden.Web.Core.Services
{
    public interface IStatsCalculator
    {
        StatsDto Calculate<T>(IEnumerable<T> values) where T : IValidatable;
    }

    public class StatsCalculator : IStatsCalculator
    {
        public StatsDto Calculate<T>(IEnumerable<T> values) where T : IValidatable
        {
            var results = values?.ToList() ?? new List<T>();
            if (!results.Any())
                return new StatsDto();

            var totalResults = (double)results.Count;
            var totalValidResults = (double)results.Count(r => r.IsValid);
            var totalInvalidResults = totalResults - totalValidResults;
            var totalUptime = totalValidResults * 100 / totalResults;
            var totalDowntime = totalInvalidResults * 100 / totalResults;

            return new StatsDto
            {
                TotalUptime = totalUptime,
                TotalDowntime = totalDowntime,
                TotalResults = totalResults,
                TotalValidResults = totalValidResults,
                TotalInvalidResults = totalInvalidResults,
            };
        }
    }
}