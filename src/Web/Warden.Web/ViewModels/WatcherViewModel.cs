using System;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Dto;

namespace Warden.Web.ViewModels
{
    public class WatcherViewModel
    {
        public Guid OrganizationId { get; set; }
        public Guid WardenId { get; set; }
        public WatcherStatsDto Stats { get; set; }
        public PagedResult<WardenCheckResultDto> Checks { get; set; }
    }
}