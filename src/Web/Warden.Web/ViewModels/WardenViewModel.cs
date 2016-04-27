using System;
using System.Collections.Generic;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Dto;

namespace Warden.Web.ViewModels
{
    public class WardenViewModel
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public WardenStatsDto Stats { get; set; }
        public PagedResult<WardenIterationDto> Iterations { get; set; }
        public IEnumerable<WatcherDto> Watchers { get; set; }
    }
}